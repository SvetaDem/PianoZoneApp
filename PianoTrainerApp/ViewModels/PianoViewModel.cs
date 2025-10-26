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
        public ObservableCollection<NoteVisual> FallingNotes { get; set; }

        private DispatcherTimer animationTimer;
        private double currentTime = 0;
        private List<MidiNote> allNotes;

        public PianoViewModel()
        {
            WhiteKeys = new ObservableCollection<PianoKey>();
            BlackKeys = new ObservableCollection<PianoKey>();
            FallingNotes = new ObservableCollection<NoteVisual>();

            GenerateKeys();

            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(20);
            animationTimer.Tick += Animate;
        }

        public void StartAnimation(List<MidiNote> notes)
        {
            allNotes = notes.OrderBy(n => n.StartTime).ToList();
            currentTime = 0;
            FallingNotes.Clear();
            animationTimer.Start();
        }

        private void Animate(object sender, EventArgs e)
        {
            currentTime += 0.02; // тикает каждые 20 мс

            // Выпускаем новые ноты, если пора
            var newNotes = allNotes
                .Where(n => n.StartTime <= currentTime && n.StartTime > currentTime - 0.02)
                .ToList();

            foreach (var n in newNotes)
            {
                FallingNotes.Add(new NoteVisual
                {
                    NoteName = n.NoteName,
                    X = GetKeyX(n.NoteName),
                    Y = -30
                });
            }

            // Двигаем все ноты вниз
            for (int i = FallingNotes.Count - 1; i >= 0; i--)
            {
                FallingNotes[i].Y += 3; // скорость падения

                if (FallingNotes[i].Y >= 200)
                    FallingNotes.RemoveAt(i);
            }
        }

        private double GetKeyX(string noteName)
        {
            var whiteKey = WhiteKeys.FirstOrDefault(k => k.Note == noteName);
            if (whiteKey != null) return whiteKey.PositionX;

            var blackKey = BlackKeys.FirstOrDefault(k => k.Note == noteName);
            if (blackKey != null) return blackKey.PositionX;

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
                    var whiteKey = new PianoKey { Note = $"{whiteNotes[i]}{octave}", IsBlack = false, PositionX = x };
                    WhiteKeys.Add(whiteKey);

                    if (!string.IsNullOrEmpty(blackNotes[i]))
                    {
                        var blackKey = new PianoKey { Note = $"{blackNotes[i]}{octave}", IsBlack = true, PositionX = x + 28 };
                        BlackKeys.Add(blackKey);
                    }

                    x += 40;
                }
            }
        }
    }
}
