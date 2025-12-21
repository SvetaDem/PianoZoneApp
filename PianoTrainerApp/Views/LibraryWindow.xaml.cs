using Microsoft.Win32;
using PianoTrainerApp.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                var tempoDialog = new TempoWindow();
                if (tempoDialog.ShowDialog() != true)
                    return;
                var pianoWindow = new PianoWindow(vm.SelectedSong, tempoDialog.SelectedMultiplier);
                if (!pianoWindow.IsInitialized)
                {
                    pianoWindow.Close();
                    return;
                }

                // копируем размеры и позиции
                pianoWindow.Width = this.Width;
                pianoWindow.Height = this.Height;
                pianoWindow.WindowState = this.WindowState;
                pianoWindow.Left = this.Left;
                pianoWindow.Top = this.Top;

                // Скрытие окна библиотеки
                this.Hide();

                // Когда пианино закроется — возврат библиотеки
                pianoWindow.Closed += PianoWindow_Closed;
                pianoWindow.Show();
            }

        }

        private void PianoWindow_Closed(object sender, EventArgs e)
        {
            var pianoWindow = sender as Window;
            if (pianoWindow == null)
                return;

            // копируем размер пианино - библиотеке
            this.Width = pianoWindow.Width;
            this.Height = pianoWindow.Height;
            this.Left = pianoWindow.Left;
            this.Top = pianoWindow.Top;
            this.WindowState = pianoWindow.WindowState;

            this.Show();
            this.Activate();
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

        private void Home_CLick(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();

            // копируем размеры и позиции
            mainWindow.Width = this.Width;
            mainWindow.Height = this.Height;
            mainWindow.WindowState = this.WindowState;
            mainWindow.Left = this.Left;
            mainWindow.Top = this.Top;

            mainWindow.Show();
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

        private void ShowFavorites_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LibraryViewModel;
            vm?.ToggleFavoritesFilter();
        }

        private void ImportMidi_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалог выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MIDI files (*.mid;*.midi)|*.mid;*.midi",
                Title = "Select a MIDI file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedPath = openFileDialog.FileName;

                // Создаем объект Song для выбранного файла (можно имя файла использовать как Title)
                var song = new PianoTrainerApp.Models.Song
                {
                    Title = System.IO.Path.GetFileNameWithoutExtension(selectedPath),
                    MidiPath = selectedPath
                };

                // Открываем окно с плывущими нотами
                var tempoDialog = new TempoWindow();
                if (tempoDialog.ShowDialog() == true)
                {
                    var pianoWindow = new PianoWindow(song, tempoDialog.SelectedMultiplier);

                    // копируем размеры и позиции
                    pianoWindow.Width = this.Width;
                    pianoWindow.Height = this.Height;
                    pianoWindow.WindowState = this.WindowState;
                    pianoWindow.Left = this.Left;
                    pianoWindow.Top = this.Top;

                    pianoWindow.Show();
                }
            }
        }
    }
}
