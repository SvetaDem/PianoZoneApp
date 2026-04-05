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
    /// Окно для отображения кастомных сообщений пользователю с поддержкой:
    /// заголовка, текста, иконки, кнопок и возвращаемого результата.
    /// Используется как замена стандартного MessageBox с возможностью стилизации.
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        /// <summary>
        /// Результат, выбранный пользователем (например, OK, Cancel, Yes, No).
        /// Доступно после закрытия окна.
        /// </summary>
        public CustomMessageBoxResult Result { get; private set; } = CustomMessageBoxResult.None;
 
        /// <summary>
        /// Конструктор окна CustomMessageBox.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="title">Заголовок окна (необязательный).</param>
        /// <param name="icon">Иконка сообщения (необязательная).</param>
        public CustomMessageBox(string message, string title = null, string icon = null)
        {
            InitializeComponent();

            // Устанавливаем текст сообщения
            MessageText.Text = message;

            // Заголовок, если есть
            if (!string.IsNullOrEmpty(title))
            {
                TitleText.Text = title;
                TitleText.Visibility = Visibility.Visible;
            }

            // Иконка, если есть
            if (!string.IsNullOrEmpty(icon))
            {
                IconText.Text = icon;
            }
        }

        // -------------------------------
        // Методы добавления кнопок
        // -------------------------------

        /// <summary>
        /// Создаёт кнопку в панели окна CustomMessageBox.
        /// </summary>
        /// <param name="text">Текст кнопки.</param>
        /// <param name="result">Результат, который вернёт окно при нажатии.</param>
        /// <param name="isPrimary">Определяет основной стиль кнопки (цвет, фон).</param>
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

            // Применяем шаблон с закруглениями и триггерами
            btn.Template = CreateButtonTemplate(isPrimary);

            // Обработчик клика: устанавливаем результат и закрываем окно
            btn.Click += (s, e) =>
            {
                Result = result;
                DialogResult = true;
                Close();
            };

            ButtonsPanel.Children.Add(btn);
        }

        /// <summary>
        /// Создаёт шаблон кнопки с закругленными углами и триггерами для наведения и нажатия.
        /// </summary>
        /// <param name="isPrimary">Основная кнопка или нет.</param>
        /// <returns>ControlTemplate для кнопки.</returns>
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

            // Триггер при наведении
            var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty,
                isPrimary ? (Brush)new BrushConverter().ConvertFrom("#5F84C9")
                          : (Brush)new BrushConverter().ConvertFrom("#E0F0FF"),
                "border"));

            // Триггер при нажатии
            var pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressedTrigger.Setters.Add(new Setter(Border.BackgroundProperty,
                isPrimary ? (Brush)new BrushConverter().ConvertFrom("#2F4F8F")
                          : (Brush)new BrushConverter().ConvertFrom("#C0E0FF"),
                "border"));

            template.Triggers.Add(hoverTrigger);
            template.Triggers.Add(pressedTrigger);

            return template;
        }

        /// <summary>
        /// Создаёт кнопки в окне в зависимости от типа CustomMessageBoxButton.
        /// </summary>
        /// <param name="buttons">Набор кнопок, которые нужно показать.</param>
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

        // -------------------------------
        // Методы работы с иконками
        // -------------------------------

        /// <summary>
        /// Настраивает отображение иконки окна.
        /// </summary>
        /// <param name="icon">Тип иконки (Info, Warning, Error, Success, Question, None).</param>
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

        // -------------------------------
        // Статический вызов окна
        // -------------------------------

        /// <summary>
        /// Статический метод для отображения окна CustomMessageBox.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="title">Заголовок окна (опционально).</param>
        /// <param name="buttons">Тип кнопок (OK, OKCancel, YesNo, YesNoCancel).</param>
        /// <param name="icon">Иконка окна (None, Info, Warning, Error, Success, Question).</param>
        /// <returns>Результат, выбранный пользователем.</returns>
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

        // Перетаскивание окна
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}