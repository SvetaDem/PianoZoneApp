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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PianoTrainerApp.ViewModels;

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class LibraryWindow : Window
    {
        public LibraryWindow()
        {
            InitializeComponent();
            DataContext = new LibraryViewModel();
        }
        private void ListBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is LibraryViewModel vm && vm.SelectedSong != null)
            {
                var pianoWin = new PianoWindow(vm.SelectedSong);
                pianoWin.Show();
            }
        }
    }
}
