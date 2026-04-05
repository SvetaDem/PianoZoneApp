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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Окно завершения песни.
    /// Выводит результат при прохождении на точность.
    /// </summary>
    public partial class SongFinishedWindow : Window
    {
        public SongFinishedWindow()
        {
            InitializeComponent();
        }

        // Обработка нажатия на кнопку рестарта игры
        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Обработка нажатия на кнопку перехода в библиотеку
        private void Library_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Перетаскивание окна
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        // Установка режима прохождения Pracrice
        public void SetPracticeMode()
        {
            PracticeText.Visibility = Visibility.Visible;
            AdvancedPanel.Visibility = Visibility.Collapsed;
        }

        // Установка режима прохождения Advanced
        public void SetAdvancedMode(double accuracy, double bestAccuracy = 0)
        {
            AdvancedPanel.Visibility = Visibility.Visible;
            PracticeText.Visibility = Visibility.Collapsed;

            double maxWidth = 268; // ширина полоски
            
            double targetWidth = maxWidth * (accuracy / 100);
            var anim = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(800)
            };

            AccuracyFill.BeginAnimation(FrameworkElement.WidthProperty, anim);

            // текущая точность
            AccuracyResultText.Text = $"{accuracy:F0}%";

            // лучшая точность
            BestAccuracyResultText.Text = $"Ваш лучший результат: {bestAccuracy:F0}%";

            // звезды
            if (accuracy >= 50) FillStar(Star50);

            if (accuracy >= 70) FillStar(Star70);

            if (accuracy >= 90) FillStar(Star90);
        }

        // Анимация заполнения звезды
        private void FillStar(Path star)
        {
            var anim = new ColorAnimation
            {
                To = (Color)ColorConverter.ConvertFromString("#FFB879"),
                Duration = TimeSpan.FromMilliseconds(300)
            };

            var brush = new SolidColorBrush(Colors.Transparent);
            star.Fill = brush;

            brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }

    }
}
