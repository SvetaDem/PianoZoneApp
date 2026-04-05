using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PianoTrainerApp.Services
{
    /// <summary>
    /// Сервис для определения высоты звука (нот) с микрофона.
    /// Использует автокорреляцию для анализа аудиосигнала
    /// и преобразует частоты в музыкальные ноты.
    /// </summary>
    public class PitchDetector
    {
        // ---------------------------
        // Настройки аудио
        // ---------------------------
        private WaveInEvent waveIn;
        private const int sampleRate = 44100;
        private const int bufferSize = 4096;

        // Событие
        public event Action<List<string>> OnNotesDetected;

        // Названия нот
        private static readonly string[] NoteNames =
        {
            "C", "C#", "D", "D#", "E", "F",
            "F#", "G", "G#", "A", "A#", "B"
        };

        // ---------------------------
        // Управление записью
        // ---------------------------
        /// <summary>
        /// Запускает захват аудио с микрофона.
        /// </summary>
        public void Start()
        {
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(sampleRate, 1),
                BufferMilliseconds = (int)(bufferSize / (double)sampleRate * 1000)
            };
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.StartRecording();
        }

        /// <summary>
        /// Останавливает запись и освобождает ресурсы.
        /// </summary>
        public void Stop()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveIn = null;
        }

        // ---------------------------
        // Обработка аудио
        // ---------------------------
        // Обрабатывает входящий аудиобуфер
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            double[] samples = new double[e.BytesRecorded / 2];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = BitConverter.ToInt16(e.Buffer, i * 2) / 32768.0;

            // RMS фильтр шума
            double rms = Math.Sqrt(samples.Select(x => x * x).Average());
            if (rms < 0.002) return;

            // Автокорреляция для поиска нот
            List<double> freqs = FindFrequencies(samples);

            // Переводим в ноты
            List<string> notes = freqs.Select(FrequencyToNote).ToList();

            if (notes.Count > 0)
                OnNotesDetected?.Invoke(notes);
        }

        // Поиск частот с помощью автокорреляции
        private List<double> FindFrequencies(double[] buffer)
        {
            List<double> detected = new List<double>();
            int minLag = sampleRate / 1000; // 1000 Hz
            int maxLag = sampleRate / 50;   // 50 Hz
            double[] autocorr = new double[maxLag + 1];

            for (int lag = minLag; lag <= maxLag; lag++)
            {
                double sum = 0;
                for (int i = 0; i < buffer.Length - lag; i++)
                    sum += buffer[i] * buffer[i + lag];
                autocorr[lag] = sum;
            }

            // Находим пики
            for (int lag = minLag + 1; lag < maxLag - 1; lag++)
            {
                if (autocorr[lag] > autocorr[lag - 1] && autocorr[lag] > autocorr[lag + 1] && autocorr[lag] > 0.01)
                {
                    double freq = sampleRate / (double)lag;
                    detected.Add(freq);
                    if (detected.Count >= 3) break; // максимум 3 ноты (аккорд)
                }
            }

            return detected;
        }

        // Преобразование частоты в музыкальную ноту
        private string FrequencyToNote(double freq)
        {
            if (freq <= 0) return "";
            double noteNumber = 69 + 12 * Math.Log(freq / 440.0, 2);
            int nearest = (int)Math.Round(noteNumber);
            int noteIndex = (nearest + 120) % 12;
            int octave = (nearest / 12) - 1;
            return $"{NoteNames[noteIndex]}{octave}";
        }
    }
}
