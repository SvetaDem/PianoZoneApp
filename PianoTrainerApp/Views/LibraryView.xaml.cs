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
using System.Windows.Controls.Primitives;
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
    /// Логика взаимодействия для LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        public LibraryView()
        {
            InitializeComponent();
            DataContext = new LibraryViewModel();
        }

        // Обработка двойного клика по элементу списка
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

                var parentWindow = Window.GetWindow(this); // получаем родителя

                // копируем размеры и позиции
                pianoWindow.Width = parentWindow.Width;
                pianoWindow.Height = parentWindow.Height;
                pianoWindow.WindowState = parentWindow.WindowState;
                pianoWindow.Left = parentWindow.Left;
                pianoWindow.Top = parentWindow.Top;

                parentWindow.Hide(); // скрываем текущее окно

                pianoWindow.Show();

                // Когда PianoWindow закроется — вернуть обратно
                pianoWindow.Closed += (s, args) =>
                {
                    parentWindow.Show();
                };
            }
        }

        // Обработка нажатия на кнопку отображения любимых песен
        private void ShowFavorites_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LibraryViewModel;
            vm?.ToggleFavoritesFilter();
        }

        // Обработка нажатия на кнопку в виде сердца (в списке)
        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button?.DataContext is Song song)
            {
                int currentUserId = Properties.Settings.Default.CurrentUserId;
                // Проверяем авторизацию
                if (currentUserId == 0)
                {
                    // Показать сообщение
                    MessageBox.Show(
                        "Добавлять треки в избранное могут только избранные ♥\nХочешь быть одним из них?) Тогда регистрируйся!",
                        "Псс…",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Отменяем переключение ToggleButton
                    button.IsChecked = false;
                    return;
                }

                // Если авторизован, продолжаем обычный код
                var vm = DataContext as LibraryViewModel;
                vm?.ToggleFavorite(song);
            }
        }

        // Обработка нажатия на кнопку для импорта MIDI-файлов
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

                    var parentWindow = Window.GetWindow(this); // получаем родителя

                    // копируем размеры и позиции
                    pianoWindow.Width = parentWindow.Width;
                    pianoWindow.Height = parentWindow.Height;
                    pianoWindow.WindowState = parentWindow.WindowState;
                    pianoWindow.Left = parentWindow.Left;
                    pianoWindow.Top = parentWindow.Top;

                    parentWindow.Hide(); // скрываем текущее окно

                    pianoWindow.Show();

                    // Когда PianoWindow закроется — вернуть обратно
                    pianoWindow.Closed += (s, args) =>
                    {
                        parentWindow.Show();
                    };
                }
            }
        }
    }
}
