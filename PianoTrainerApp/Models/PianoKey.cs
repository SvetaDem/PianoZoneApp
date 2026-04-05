using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    /// <summary>
    /// Модель клавиши пианино.
    /// Представляет отдельную клавишу (белую или черную) с информацией о ноте,
    /// позиции на клавиатуре и состоянии нажатия.
    /// </summary>
    public class PianoKey : INotifyPropertyChanged
    {
        public string Note { get; set; }
        public bool IsBlack { get; set; }  // Черная/Белая клавиша
        public double PositionX { get; set; } // Позиция клавиши по оси X (используется для размещения черных клавиш над белыми)
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
