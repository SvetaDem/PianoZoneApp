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
    }
}
