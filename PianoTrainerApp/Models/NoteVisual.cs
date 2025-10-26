using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    public class NoteVisual
    {
        public string NoteName { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Duration { get; set; } // длительность в секундах public double StartTime { get; set; } // момент появления
    }
}
