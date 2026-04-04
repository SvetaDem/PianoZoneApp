using PianoTrainerApp.Models;
using PianoTrainerApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для LessonsView.xaml
    /// </summary>
    public partial class LessonsView : UserControl
    {
        public LessonsView()
        {
            InitializeComponent();

            DataContext = new LessonsViewModel();
        }

        private void PianoGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null)
            {
                var clipRect = new RectangleGeometry()
                {
                    Rect = new Rect(0, 0, grid.ActualWidth, grid.ActualHeight),
                    RadiusX = 15, // совпадает с CornerRadius
                    RadiusY = 15
                };
                grid.Clip = clipRect;
            }
        }

        private void Note_Click(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb == null) return;

            string note = "C";
            string noteHeader = "До 1 октавы"; // заголовок по умолчанию

            switch (tb.Text)
            {
                // Белые клавиши
                case "До": note = "C"; noteHeader = "До 1 октавы"; break;
                case "Ре": note = "D"; noteHeader = "Ре 1 октавы"; break;
                case "Ми": note = "E"; noteHeader = "Ми 1 октавы"; break;
                case "Фа": note = "F"; noteHeader = "Фа 1 октавы"; break;
                case "Соль": note = "G"; noteHeader = "Соль 1 октавы"; break;
                case "Ля": note = "A"; noteHeader = "Ля 1 октавы"; break;
                case "Си": note = "B"; noteHeader = "Си 1 октавы"; break;

                // Чёрные клавиши (диезы)
                case "До#": note = "C#"; noteHeader = "До-диез / Ре-бемоль 1 октавы"; break;
                case "Ре#": note = "D#"; noteHeader = "Ре-диез / Ми-бемоль 1 октавы"; break;
                case "Фа#": note = "F#"; noteHeader = "Фа-диез / Соль-бемоль 1 октавы"; break;
                case "Соль#": note = "G#"; noteHeader = "Соль-диез / Ля-бемоль 1 октавы"; break;
                case "Ля#": note = "A#"; noteHeader = "Ля-диез / Си-бемоль 1 октавы"; break;
            }

            var vm = DataContext as LessonsViewModel;
            if (vm != null)
            {
                vm.SelectedNote = note;
                vm.NoteHeader = noteHeader; // сразу обновляем заголовок
            }
        }
    }
}
