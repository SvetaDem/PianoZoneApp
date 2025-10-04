using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp
{
    public class Song
    {
        public string Title { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public string MidiPath { get; set; } // путь к файлу
    }
}
