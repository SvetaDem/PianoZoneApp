using PianoTrainerApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для PianoWindow.xaml
    /// </summary>
    public partial class PianoWindow : Window
    {
        private Song currentSong;

        public PianoWindow(Song song)
        {
            InitializeComponent();
            currentSong = song;
            Title = $"Тренировка: {song.Title}";
            DataContext = new PianoViewModel();
        }
    }
}
