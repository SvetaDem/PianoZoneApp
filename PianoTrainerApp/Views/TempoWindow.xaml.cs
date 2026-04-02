using PianoTrainerApp.Models;
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
    /// Логика взаимодействия для TempoWindow.xaml
    /// </summary>
    public partial class TempoWindow : Window
    {
        public double SelectedMultiplier { get; private set; } = 1.0;
        public PlayMode SelectedMode { get; private set; } = PlayMode.Advanced;

        public TempoWindow()
        {
            InitializeComponent();
            SetAdvancedMode();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (PracticePanel.Visibility == Visibility.Visible)
            {
                SelectedMultiplier = TempoSlider.Value;
                SelectedMode = PlayMode.Practice;
            }
            else
            {
                // Проверяем авторизацию для Advanced режима
                int currentUserId = Properties.Settings.Default.CurrentUserId;
                if (currentUserId == 0)
                {
                    CustomMessageBox.Show(
                        "Режим Продвинутый доступен только для зарегистрированных пользователей\nАвторизуйся, чтобы можно было играть на точность попаданий!",
                        "Эй…",
                        CustomMessageBoxButton.OK,
                        CustomMessageBoxImage.Info
                    );
                    return; // Не закрываем окно
                }

                SelectedMultiplier = 1.0;
                SelectedMode = PlayMode.Advanced;
            }

            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SetAdvancedMode()
        {
            AdvancedTab.Background = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
            PracticeTab.Background = Brushes.Transparent;
            AdvancedTabText.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFFFFF");
            PracticeTabText.Foreground = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
            PracticeTab.BorderThickness = new Thickness(0, 1, 1, 1);
            PracticeTab.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#3F64A9");

            AdvancedImage.Visibility = Visibility.Visible;
            PracticePanel.Visibility = Visibility.Collapsed;

            ModeText.Text = "Играй на точность попаданий и получай звёзды!";
        }

        private void SetPracticeMode()
        {
            PracticeTab.Background = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
            AdvancedTab.Background = Brushes.Transparent;
            AdvancedTabText.Foreground = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
            PracticeTabText.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFFFFF");
            AdvancedTab.BorderThickness = new Thickness(1, 1, 0, 1);
            AdvancedTab.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#3F64A9");

            AdvancedImage.Visibility = Visibility.Collapsed;
            PracticePanel.Visibility = Visibility.Visible;

            ModeText.Text = "Практикуйся в игре на своём инструменте в любом темпе, без оценки!";
        }

        // клики
        private void AdvancedTab_Click(object sender, MouseButtonEventArgs e)
        {
            SetAdvancedMode();
        }

        private void PracticeTab_Click(object sender, MouseButtonEventArgs e)
        {
            SetPracticeMode();
        }

        // перетаскивание
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
