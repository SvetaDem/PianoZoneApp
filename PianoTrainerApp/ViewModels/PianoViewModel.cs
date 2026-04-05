using PianoTrainerApp.Models;
using PianoTrainerApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PianoTrainerApp.ViewModels
{
    /// <summary>
    /// ViewModel пианино.
    /// Отвечает за генерацию клавиатуры, анимацию падающих нот,
    /// обработку нажатий и логику воспроизведения (Practice / Advanced).
    /// </summary>
    public class PianoViewModel
    {
        // ---------------------------
        // Клавиатура пианино
        // ---------------------------
        public ObservableCollection<PianoKey> WhiteKeys { get; set; }
        public ObservableCollection<PianoKey> BlackKeys { get; set; }
        
        // ---------------------------
        // Ноты
        // ---------------------------
        public ObservableCollection<MidiNote> FallingNotes { get; set; }
        public List<MidiNote> OriginalNotes { get; private set; }

        // ---------------------------
        // Настройки воспроизведения
        // ---------------------------
        public double SpeedMultiplier { get; set; } = 1.0; // 1x по умолчанию
        public PlayMode Mode { get; set; } = PlayMode.Practice;

        // ---------------------------
        // Аккорды и состояние
        // ---------------------------
        public List<MidiNote> WaitingChord { get; set; } = new List<MidiNote>();

        private DispatcherTimer animationTimer;
        private List<MidiNote> allNotes;
        private DateTime startTime;
        public double CurrentTime => (DateTime.Now - startTime).TotalSeconds;

        public bool IsSongFinished
        {
            get
            {
                if (Mode == PlayMode.Advanced)
                {
                    // В Advanced — просто проверяем, все ли ноты дошли до конца (HasCompleted)
                    return FallingNotes.All(n => n.HasCompleted);
                }
                else
                {
                    // В Practice — проверяем ноты + WaitingChord
                    return !FallingNotes.Any(n => !n.HasCompleted) && !WaitingChord.Any();
                }
            }
        }

        /// <summary>
        /// Инициализирует клавиатуру и таймер анимации.
        /// </summary>
        public PianoViewModel()
        {
            WhiteKeys = new ObservableCollection<PianoKey>();
            BlackKeys = new ObservableCollection<PianoKey>();
            FallingNotes = new ObservableCollection<MidiNote>();

            GenerateKeys();

            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(20);
        }

        // ---------------------------
        // Анимация
        // ---------------------------
        /// <summary>
        /// Запускает анимацию падающих нот.
        /// </summary>
        public void StartAnimation(List<MidiNote> notes)
        {
            // Сохраняем оригинальные ноты для перезапуска
            OriginalNotes = notes.Select(n => new MidiNote
            {
                NoteName = n.NoteName,
                StartTime = n.StartTime,
                Duration = n.Duration,
                X = n.X
            }).ToList();

            FallingNotes.Clear();
            startTime = DateTime.Now;

            // Назначаем с X позиции
            foreach (var n in notes)
                n.X = GetKeyX(n.NoteName);

            allNotes = notes.OrderBy(n => n.StartTime).ToList();

            // Применяем SpeedMultiplier сразу при импорте, чтобы ноты не наслаивались
            foreach (var n in allNotes)
            {
                n.StartTime /= SpeedMultiplier;
                n.Duration /= SpeedMultiplier;
            }

            // Добавляем все ноты сразу
            foreach (var n in allNotes)
                FallingNotes.Add(n);

            animationTimer.Start();
        }

        // Получение X позиции клавиши
        private double GetKeyX(string noteName)
        {
            var whiteKey = WhiteKeys.FirstOrDefault(k => k.Note == noteName);
            if (whiteKey != null)
                return whiteKey.PositionX;
            var blackKey = BlackKeys.FirstOrDefault(k => k.Note == noteName);
            if (blackKey != null)
                return blackKey.PositionX;
            return 0;
        }

        // Генерация клавиш пианино
        private void GenerateKeys()
        {
            string[] whiteNotes = { "C", "D", "E", "F", "G", "A", "B" };
            string[] blackNotes = { "C#", "D#", "", "F#", "G#", "A#", "" };

            double x = 0;

            for (int octave = 1; octave <= 7; octave++)
            {
                for (int i = 0; i < 7; i++)
                {
                    WhiteKeys.Add(new PianoKey
                    {
                        Note = $"{whiteNotes[i]}{octave}",
                        IsBlack = false,
                        PositionX = x
                    });
                    if (!string.IsNullOrEmpty(blackNotes[i]))
                        BlackKeys.Add(new PianoKey
                        {
                            Note = $"{blackNotes[i]}{octave}",
                            IsBlack = true,
                            PositionX = x + 28
                        }); x += 40;
                }
            }

        }

        // ------------------------------
        //  Подсветка клавиш по падению нот
        // ------------------------------
        public void PressKey(string noteName)
        {
            var key = FindKey(noteName);
            if (key != null) key.IsPressed = true;
        }

        public void ReleaseKey(string noteName)
        {
            var key = FindKey(noteName);
            if (key != null) key.IsPressed = false;
        }

        /// <summary>
        /// Подсвечивает аккорд на основе распознанных нот (например, с микрофона).
        /// </summary>
        public void HighlightChord(List<string> detectedNotes)
        {
            foreach (var key in WhiteKeys.Concat(BlackKeys))
            {
                if (WaitingChord.Any(n => n.NoteName == key.Note))
                    key.IsCorrectlyPlayed = detectedNotes.Contains(key.Note);
                else
                    key.IsCorrectlyPlayed = null;
            }
        }

        // ------------------------------
        // Вспомогательные методы
        // ------------------------------
        // Поиск клавиши по названию ноты
        private PianoKey FindKey(string note)
        {
            return WhiteKeys.Concat(BlackKeys)
                            .FirstOrDefault(k => k.Note == note);
        }

        /// <summary>
        /// Корректирует время старта после паузы.
        /// </summary>
        public void AdjustStartTimeForPause(TimeSpan pauseDuration)
        {
            startTime = startTime.Add(pauseDuration);
        }

        /// <summary>
        /// Сбрасывает состояние воспроизведения и нот.
        /// </summary>
        public void Reset()
        {
            startTime = DateTime.Now;  // сброс времени
            WaitingChord.Clear();

            foreach (var note in FallingNotes)
            {
                note.HasCompleted = false;
                note.HasPressed = false;
            }
        }
    }
}
