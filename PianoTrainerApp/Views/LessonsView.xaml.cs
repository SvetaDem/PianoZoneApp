using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using PianoTrainerApp.Models;
using PianoTrainerApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для LessonsView.xaml
    /// </summary>
    public partial class LessonsView : UserControl
    {
        public LessonsView()
        {
            InitializeComponent();

            DataContext = new LessonsViewModel();
        }

        private void PianoGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null)
            {
                var clipRect = new RectangleGeometry()
                {
                    Rect = new Rect(0, 0, grid.ActualWidth, grid.ActualHeight),
                    RadiusX = 15, // совпадает с CornerRadius
                    RadiusY = 15
                };
                grid.Clip = clipRect;
            }
        }

        private void Note_Click(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb == null) return;

            string note = "C";
            string noteHeader = "До 1 октавы"; // заголовок по умолчанию

            switch (tb.Text)
            {
                // Белые клавиши
                case "До": note = "C"; noteHeader = "До 1 октавы"; break;
                case "Ре": note = "D"; noteHeader = "Ре 1 октавы"; break;
                case "Ми": note = "E"; noteHeader = "Ми 1 октавы"; break;
                case "Фа": note = "F"; noteHeader = "Фа 1 октавы"; break;
                case "Соль": note = "G"; noteHeader = "Соль 1 октавы"; break;
                case "Ля": note = "A"; noteHeader = "Ля 1 октавы"; break;
                case "Си": note = "B"; noteHeader = "Си 1 октавы"; break;

                // Чёрные клавиши (диезы)
                case "До#": note = "C#"; noteHeader = "До-диез / Ре-бемоль 1 октавы"; break;
                case "Ре#": note = "D#"; noteHeader = "Ре-диез / Ми-бемоль 1 октавы"; break;
                case "Фа#": note = "F#"; noteHeader = "Фа-диез / Соль-бемоль 1 октавы"; break;
                case "Соль#": note = "G#"; noteHeader = "Соль-диез / Ля-бемоль 1 октавы"; break;
                case "Ля#": note = "A#"; noteHeader = "Ля-диез / Си-бемоль 1 октавы"; break;
            }

            var vm = DataContext as LessonsViewModel;
            if (vm != null)
            {
                vm.SelectedNote = note;
                vm.NoteHeader = noteHeader; // сразу обновляем заголовок
            }
        }

        // Метод для воспроизведения звука (почти как звук пианино)
        private void PlayPianoTone(double frequency, int durationMs = 520)
        {
            var waveOut = new WaveOutEvent();  // WaveOutEvent — это объект, который умеет воспроизводить звук через колонки.
                                               // Он работает асинхронно, можно многократно запускать разные звуки.
            
            // Основной тон
            var main = new SignalGenerator()  // генератор сигнала
            {
                Gain = 0.3,              // громкость (0-1)
                Frequency = frequency,   // частота звука в Гц
                Type = SignalGeneratorType.Sin // синусоида
            };

            // Обертоны для звонкости
            var overtone1 = new SignalGenerator()
            {
                Gain = 0.15,
                Frequency = frequency * 2,
                Type = SignalGeneratorType.Sin
            };
            var overtone2 = new SignalGenerator()
            {
                Gain = 0.1,
                Frequency = frequency * 3,
                Type = SignalGeneratorType.Sin
            };

            // Смешиваем все генераторы
            var mixer = new MixingSampleProvider(new ISampleProvider[]
            {
                main,
                overtone1,
                overtone2
            });

            // Плавное появление и исчезновение
            var fade = new FadeInOutSampleProvider(mixer, true);
            fade.BeginFadeIn(10); // 10 мс плавного появления
            fade.BeginFadeOut(durationMs - 10); // 10 мс плавного затухания

            // Ограничиваем длительность звука
            var take = new OffsetSampleProvider(fade).Take(TimeSpan.FromMilliseconds(durationMs));  // берём только первые durationMs миллисекунд.
                                                                                                    // То есть, если durationMs = 500 - звук играет 0.5 секунды.

            waveOut.Init(take);  // подключаем наш сигнал к устройству
            waveOut.Play();      // воспроизводим звук
        }

        // Метод получения частоты ноты
        private double GetFrequency(string note)
        {
            switch (note)
            {
                case "C": return 261.63;
                case "C#": return 277.18;
                case "D": return 293.66;
                case "D#": return 311.13;
                case "E": return 329.63;
                case "F": return 349.23;
                case "F#": return 369.99;
                case "G": return 392.00;
                case "G#": return 415.30;
                case "A": return 440.00;
                case "A#": return 466.16;
                case "B": return 493.88;
                default: return 0;
            }
        }

        // обработчик кнопки воспроизведения звука
        private void PlayNote_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LessonsViewModel;
            if (vm == null) return;

            double freq = GetFrequency(vm.SelectedNote);
            if (freq > 0)
                PlayPianoTone(freq);
        }
    }
}
