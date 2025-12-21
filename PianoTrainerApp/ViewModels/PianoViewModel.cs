using PianoTrainerApp.Models;
using PianoTrainerApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PianoTrainerApp.ViewModels
{
    public class PianoViewModel
    {
        public ObservableCollection<PianoKey> WhiteKeys { get; set; }
        public ObservableCollection<PianoKey> BlackKeys { get; set; }
        public ObservableCollection<MidiNote> FallingNotes { get; set; }

        public List<MidiNote> OriginalNotes { get; private set; }

        public double SpeedMultiplier { get; set; } = 1.0; // 1x по умолчанию

        public List<MidiNote> WaitingChord { get; set; } = new List<MidiNote>();

        private DispatcherTimer animationTimer;
        private List<MidiNote> allNotes;
        private DateTime startTime;
        public double CurrentTime => (DateTime.Now - startTime).TotalSeconds;

        public bool IsSongFinished => !FallingNotes.Any(n => !n.HasCompleted) && !WaitingChord.Any();

        public PianoViewModel()
        {
            WhiteKeys = new ObservableCollection<PianoKey>();
            BlackKeys = new ObservableCollection<PianoKey>();
            FallingNotes = new ObservableCollection<MidiNote>();

            GenerateKeys();

            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(20);
        }

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
        //  Подсветка по падению нот
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

        // Подсветка по микрофону (зелёный/красный)
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
        //  Найти клавишу по названию ноты
        // ------------------------------
        private PianoKey FindKey(string note)
        {
            return WhiteKeys.Concat(BlackKeys)
                            .FirstOrDefault(k => k.Note == note);
        }

        public void AdjustStartTimeForPause(TimeSpan pauseDuration)
        {
            startTime = startTime.Add(pauseDuration);
        }

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
