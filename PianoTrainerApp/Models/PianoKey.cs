using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    public class PianoKey : INotifyPropertyChanged
    {
        public string Note { get; set; }
        public bool IsBlack { get; set; }  // Черная/Белая клавиша
        public double PositionX { get; set; } // Для чёрных клавиш
        private bool isPressed;
        public bool IsPressed
        {
            get => isPressed;
            set { isPressed = value; OnPropertyChanged(nameof(IsPressed)); }
        }

        private bool? isCorrect;
        public bool? IsCorrectlyPlayed
        {
            get => isCorrect;
            set { isCorrect = value; OnPropertyChanged(nameof(IsCorrectlyPlayed)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
