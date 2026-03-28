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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBoxResult Result { get; private set; } = CustomMessageBoxResult.None;

        public CustomMessageBox(string message, string title = null, string icon = null)
        {
            InitializeComponent();

            MessageText.Text = message;

            if (!string.IsNullOrEmpty(title))
            {
                TitleText.Text = title;
                TitleText.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                IconText.Text = icon;
            }
        }

        // --- ДОБАВЛЕНИЕ КНОПОК ---
        private void AddButton(string text, CustomMessageBoxResult result, bool isPrimary = false)
        {
            var btn = new Button
            {
                Content = text,
                Width = 90,
                Height = 34,
                Margin = new Thickness(6, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            if (isPrimary)
            {
                btn.Background = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
                btn.Foreground = Brushes.White;
                btn.BorderThickness = new Thickness(0);
            }
            else
            {
                btn.Background = Brushes.Transparent;
                btn.Foreground = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
                btn.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
                btn.BorderThickness = new Thickness(1.5);
            }

            btn.Template = CreateButtonTemplate(isPrimary);

            btn.Click += (s, e) =>
            {
                Result = result;
                DialogResult = true;
                Close();
            };

            ButtonsPanel.Children.Add(btn);
        }

        // Шаблон кнопки
        private ControlTemplate CreateButtonTemplate(bool isPrimary)
        {
            var template = new ControlTemplate(typeof(Button));

            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(12));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(content);
            template.VisualTree = border;

            // 🔥 ТРИГГЕРЫ
            var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty,
                isPrimary ? (Brush)new BrushConverter().ConvertFrom("#5F84C9")
                          : (Brush)new BrushConverter().ConvertFrom("#E0F0FF"),
                "border"));

            var pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressedTrigger.Setters.Add(new Setter(Border.BackgroundProperty,
                isPrimary ? (Brush)new BrushConverter().ConvertFrom("#2F4F8F")
                          : (Brush)new BrushConverter().ConvertFrom("#C0E0FF"),
                "border"));

            template.Triggers.Add(hoverTrigger);
            template.Triggers.Add(pressedTrigger);

            return template;
        }

        private void CreateButtons(CustomMessageBoxButton buttons)
        {
            switch (buttons)
            {
                case CustomMessageBoxButton.OK:
                    AddButton("ОК", CustomMessageBoxResult.OK, true);
                    break;

                case CustomMessageBoxButton.OKCancel:
                    AddButton("ОК", CustomMessageBoxResult.OK, true);
                    AddButton("Отмена", CustomMessageBoxResult.Cancel);
                    break;

                case CustomMessageBoxButton.YesNo:
                    AddButton("Да", CustomMessageBoxResult.Yes, true);
                    AddButton("Нет", CustomMessageBoxResult.No);
                    break;

                case CustomMessageBoxButton.YesNoCancel:
                    AddButton("Да", CustomMessageBoxResult.Yes, true);
                    AddButton("Нет", CustomMessageBoxResult.No);
                    AddButton("Отмена", CustomMessageBoxResult.Cancel);
                    break;
            }
        }

        // Настройка иконок
        private void SetIcon(CustomMessageBoxImage icon)
        {
            IconText.Visibility = Visibility.Visible;

            switch (icon)
            {
                case CustomMessageBoxImage.None:
                    IconText.Visibility = Visibility.Collapsed;
                    break;

                case CustomMessageBoxImage.Info:
                    IconText.Text = "ℹ";
                    IconText.Foreground = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
                    break;

                case CustomMessageBoxImage.Warning:
                    IconText.Text = "⚠";
                    IconText.Foreground = Brushes.DarkOrange;
                    break;

                case CustomMessageBoxImage.Error:
                    IconText.Text = "✖";
                    IconText.Foreground = Brushes.Red;
                    break;

                case CustomMessageBoxImage.Success:
                    IconText.Text = "✔";
                    IconText.Foreground = Brushes.Green;
                    break;

                case CustomMessageBoxImage.Question:
                    IconText.Text = "?";
                    IconText.Foreground = (Brush)new BrushConverter().ConvertFrom("#3F64A9");
                    break;
            }
        }

        // --- СТАТИЧЕСКИЙ ВЫЗОВ ---
        public static CustomMessageBoxResult Show(
            string message,
            string title = null,
            CustomMessageBoxButton buttons = CustomMessageBoxButton.OK,
            CustomMessageBoxImage icon = CustomMessageBoxImage.None
            )
        {
            var box = new CustomMessageBox(message, title);
            box.SetIcon(icon);
            box.CreateButtons(buttons);

            box.ShowDialog();
            return box.Result;
        }

        // --- ПЕРЕТАСКИВАНИЕ ---
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}