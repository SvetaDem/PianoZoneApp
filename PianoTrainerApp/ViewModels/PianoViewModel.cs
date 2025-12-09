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

        public double SpeedMultiplier { get; set; } = 1.0; // 1x по умолчанию


        private DispatcherTimer animationTimer;
        private List<MidiNote> allNotes;
        private DateTime startTime;
        public double CurrentTime => (DateTime.Now - startTime).TotalSeconds;

       
        public PianoViewModel()
        {
            WhiteKeys = new ObservableCollection<PianoKey>();
            BlackKeys = new ObservableCollection<PianoKey>();
            FallingNotes = new ObservableCollection<MidiNote>();

            GenerateKeys();

            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(20);
            //animationTimer.Tick += Animate;
        }
        public void StartAnimation(List<MidiNote> notes)
        {

            FallingNotes.Clear();
            startTime = DateTime.Now;

            // Назначаем с X позиции
            foreach (var n in notes)
                n.X = GetKeyX(n.NoteName);

            allNotes = notes.OrderBy(n => n.StartTime).ToList();

            // Добавляем все ноты сразу
            foreach (var n in allNotes)
                FallingNotes.Add(n);

            animationTimer.Start();
        }


        /*private void Animate(object sender, EventArgs e)
        {
            // удаляем ноты, которые полностью ушли за экран
            *//*for (int i = FallingNotes.Count - 1; i >= 0; i--)
            {
                var note = FallingNotes[i];
                if (CurrentTime - note.StartTime > note.Duration + 5)
                    FallingNotes.RemoveAt(i);
            }*//*

            for (int i = FallingNotes.Count - 1; i >= 0; i--)
            {
                var note = FallingNotes[i];

                double deathTime = note.StartTime + note.Duration + 5;

                Console.WriteLine(
                    $"[{note.NoteName}] duration = {note.Duration}, " +
                    $"умрёт в = {deathTime:F2}, сейчас = {CurrentTime:F2}"
                );

                if (CurrentTime > deathTime)
                    FallingNotes.RemoveAt(i);
            }
        }*/

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

        public void PressKey(string noteName)
        {
            var key = WhiteKeys.Concat(BlackKeys).FirstOrDefault(k => k.Note == noteName);
            if (key != null) key.IsPressed = true;
        }

        public void ReleaseKey(string noteName)
        {
            var key = WhiteKeys.Concat(BlackKeys).FirstOrDefault(k => k.Note == noteName);
            if (key != null) key.IsPressed = false;
        }

    }
}
