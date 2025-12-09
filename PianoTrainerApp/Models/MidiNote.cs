using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    public class MidiNote
    {
        public string NoteName { get; set; }   // Например "C4"
        public double StartTime { get; set; }  // Время появления (в секундах)
        public double Duration { get; set; }   // Длительность (сек)
        public double X { get; set; } // позиция по X (будет назначена при старте)
        public bool HasPressed { get; set; } = false;
    }
}
