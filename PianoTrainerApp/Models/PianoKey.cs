using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    public class PianoKey
    {
        public string Note { get; set; }
        public bool IsBlack { get; set; }  // Черная/Белая клавиша
        public double PositionX { get; set; } // Для чёрных клавиш
    }
}
