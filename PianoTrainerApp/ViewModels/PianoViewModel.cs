using PianoTrainerApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.ViewModels
{
    public class PianoViewModel
    {
        public ObservableCollection<PianoKey> WhiteKeys { get; set; }
        public ObservableCollection<PianoKey> BlackKeys { get; set; }

        public PianoViewModel()
        {
            WhiteKeys = new ObservableCollection<PianoKey>();
            BlackKeys = new ObservableCollection<PianoKey>();

            GenerateKeys();
        }

        private void GenerateKeys()
        {
            string[] whiteNotes = { "C", "D", "E", "F", "G", "A", "B" };
            string[] blackNotes = { "C#", "D#", "", "F#", "G#", "A#", "" };

            int whiteKeyIndex = 0;
            double x = 0;
            for (int octave = 1; octave <= 7; octave++)
            {
                for (int i = 0; i < 7; i++)
                {
                    // Белая клавиша
                    var whiteKey = new PianoKey
                    {
                        Note = $"{whiteNotes[i]}{octave}",
                        IsBlack = false
                    };
                    WhiteKeys.Add(whiteKey);

                    // Чёрная клавиша, если есть
                    if (!string.IsNullOrEmpty(blackNotes[i]))
                    {
                        var blackKey = new PianoKey
                        {
                            Note = "",
                            IsBlack = true,
                            PositionX = x + 27 // чуть смещаем внутрь белых
                        };
                        BlackKeys.Add(blackKey);
                    }

                    x += 40; // ширина белой клавиши
                    whiteKeyIndex++;
                }
            }
        }
    }
}
