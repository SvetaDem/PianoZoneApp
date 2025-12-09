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
        private bool isPressed;
        public string Note { get; set; }
        public bool IsBlack { get; set; }  // Черная/Белая клавиша
        public double PositionX { get; set; } // Для чёрных клавиш
        public bool IsPressed
        {
            get => isPressed;
            set
            {
                if (isPressed != value)
                {
                    isPressed = value;
                    OnPropertyChanged(nameof(IsPressed));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
