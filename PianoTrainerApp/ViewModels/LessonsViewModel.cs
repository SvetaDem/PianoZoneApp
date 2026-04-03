using PianoTrainerApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PianoTrainerApp.ViewModels
{
    public class LessonsViewModel
    {
        public ObservableCollection<PianoKey> WhiteKeys { get; set; }
        public ObservableCollection<PianoKey> BlackKeys { get; set; }
        public LessonsViewModel()
        {
            WhiteKeys = new ObservableCollection<PianoKey>();
            BlackKeys = new ObservableCollection<PianoKey>();

            GenerateKeys();
            SelectNote("C");
        }

        private void GenerateKeys()
        {
            WhiteKeys.Clear();
            BlackKeys.Clear();

            string[] whiteNotes = { "C", "D", "E", "F", "G", "A", "B" };
            string[] blackNotes = { "C#", "D#", "", "F#", "G#", "A#", "", };

            double x = 0;

            for (int i = 0; i < whiteNotes.Length; i++)
            {
                WhiteKeys.Add(new PianoKey
                {
                    Note = whiteNotes[i],
                    IsBlack = false,
                    PositionX = x
                });

                if (!string.IsNullOrEmpty(blackNotes[i]))
                {
                    BlackKeys.Add(new PianoKey
                    {
                        Note = blackNotes[i],
                        IsBlack = true,
                        PositionX = x + 28
                    });
                }

                x += 40;
            }
        }

        public void SelectNote(string note)
        {
            foreach (var key in WhiteKeys.Concat(BlackKeys))
                key.IsPressed = key.Note == note;
        }

    }
}
