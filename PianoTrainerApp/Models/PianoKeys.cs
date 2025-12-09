using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    public class PianoKeys
    {
        /*public ObservableCollection<PianoKey> WhiteKeys { get; set; } = new();
        public ObservableCollection<PianoKey> BlackKeys { get; set; } = new();

        public ObservableCollection<PianoKey> AllKeys { get; set; } = new();*/
    }

   /* public static class PianoGenerator
    {
        public static PianoKeys GenerateKeys()
        {
            var piano = new PianoKeys();

            string[] whiteNotes = { "C", "D", "E", "F", "G", "A", "B" };
            string[] blackNotes = { "C#", "D#", "", "F#", "G#", "A#", "" };

            double x = 0;

            for (int octave = 1; octave <= 7; octave++)
            {
                for (int i = 0; i < 7; i++)
                {
                    // Белые клавиши
                    var whiteKey = new PianoKey
                    {
                        Note = $"{whiteNotes[i]}{octave}",
                        IsBlack = false,
                        PositionX = x
                    };
                    piano.WhiteKeys.Add(whiteKey);
                    piano.AllKeys.Add(whiteKey);

                    // Чёрные клавиши
                    if (!string.IsNullOrEmpty(blackNotes[i]))
                    {
                        var blackKey = new PianoKey
                        {
                            Note = $"{blackNotes[i]}{octave}",
                            IsBlack = true,
                            PositionX = x + 28 // чуть правее белой
                        };
                        piano.BlackKeys.Add(blackKey);
                        piano.AllKeys.Add(blackKey);
                    }

                    x += 40; // шаг по X для белых клавиш
                }
            }

            return piano;
        }
    }*/
}
