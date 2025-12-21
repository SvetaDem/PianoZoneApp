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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBeginner_Click(object sender, RoutedEventArgs e)
        {
           //
        }

        private void ButtonLibrary_Click(object sender, RoutedEventArgs e)
        {
            var libraryWindow = new LibraryWindow();

            // копируем размеры и позиции
            libraryWindow.Width = this.Width;
            libraryWindow.Height = this.Height;
            libraryWindow.WindowState = this.WindowState;
            libraryWindow.Left = this.Left;
            libraryWindow.Top = this.Top;

            libraryWindow.Show();
            this.Close();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы точно хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
                );

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
