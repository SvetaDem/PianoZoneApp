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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User currentUser;
        
        public MainWindow()
        {
            InitializeComponent();
            AutoLogin();
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

        bool profileOpened = false;
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            double to = profileOpened ? 350 : 0;

            var anim = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            ProfilePanelTransform.BeginAnimation(TranslateTransform.XProperty, anim);

            profileOpened = !profileOpened;

            Overlay.Visibility = profileOpened ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!profileOpened) return;

            var anim = new DoubleAnimation
            {
                To = 350,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            ProfilePanelTransform.BeginAnimation(TranslateTransform.XProperty, anim);

            profileOpened = false;
            Overlay.Visibility = Visibility.Collapsed;
        }

        private void RegisterTextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            // Скрываем панель авторизации
            AuthPanel.Visibility = Visibility.Collapsed;

            // Показываем панель регистрации
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем панель регистрации
            RegisterPanel.Visibility = Visibility.Collapsed;

            // Показываем панель авторизации
            AuthPanel.Visibility = Visibility.Visible;
        }

        private string realAuthPassword = "";
        private string realRegisterPassword = "";

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            using (var db = new ReMinorContext())
            {
                // ---------- РЕГИСТРАЦИЯ ----------
                if (button.Name == "RegisterButton")
                {
                    string username = RegisterUsernameTextBox.Text.Trim();
                    string email = RegisterEmailTextBox.Text.Trim();
                    string password = realRegisterPassword; // используем реальный пароль

                    // проверка пустых полей
                    if (string.IsNullOrWhiteSpace(username) ||
                        string.IsNullOrWhiteSpace(email) ||
                        string.IsNullOrWhiteSpace(password))
                    {
                        MessageBox.Show("Все поля должны быть заполнены");
                        return;
                    }

                    // проверка уникальности логина
                    var existingUser = db.Users.FirstOrDefault(u => u.Username == username);
                    if (existingUser != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует");
                        return;
                    }

                    // проверка сложности пароля
                    if (!IsValidPassword(password))
                    {
                        MessageBox.Show("Пароль должен быть минимум 6 символов, содержать заглавную букву, строчную и цифру");
                        return;
                    }

                    string passwordHash = PasswordHelper.HashPassword(password);

                    var user = new User
                    {
                        Username = username,
                        Email = email,
                        PasswordHash = passwordHash
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    currentUser = user;
                }

                // ---------- АВТОРИЗАЦИЯ ----------
                else if (button.Name == "AuthButton")
                {
                    string username = AuthUsernameTextBox.Text.Trim();
                    string password = realAuthPassword; // используем реальный пароль
                    // проверка пустых полей
                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        MessageBox.Show("Введите логин и пароль");
                        return;
                    }

                    string passwordHash = PasswordHelper.HashPassword(password);

                    var user = db.Users.FirstOrDefault(u =>
                        u.Username == username &&
                        u.PasswordHash == passwordHash);

                    if (user == null)
                    {
                        MessageBox.Show("Неверный логин или пароль");
                        return;
                    }

                    currentUser = user;
                }
            }

            // ---------- АВТОЛОГИН ----------
            Properties.Settings.Default.CurrentUserId = currentUser.Id;
            Properties.Settings.Default.Save();

            // ---------- ОТОБРАЖЕНИЕ ПРОФИЛЯ ----------
            UsernameProfileText.Text = currentUser.Username;

            RegisterPanel.Visibility = Visibility.Collapsed;
            AuthPanel.Visibility = Visibility.Collapsed;
            UserPanel.Visibility = Visibility.Visible;
        }

        // ---------- Вспомогательный метод для проверки сложности пароля ----------
        private bool IsValidPassword(string password)
        {
            if (password.Length < 6) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            return true;
        }

        private void AutoLogin()
        {
            int savedUserId = Properties.Settings.Default.CurrentUserId;

            if (savedUserId == 0)
                return;

            using (var db = new ReMinorContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == savedUserId);

                if (user == null)
                    return;

                currentUser = user;

                UsernameProfileText.Text = user.Username;

                AuthPanel.Visibility = Visibility.Collapsed;
                RegisterPanel.Visibility = Visibility.Collapsed;
                UserPanel.Visibility = Visibility.Visible;
            }
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CurrentUserId = 0;
            Properties.Settings.Default.Save();

            currentUser = null;

            UserPanel.Visibility = Visibility.Collapsed;
            AuthPanel.Visibility = Visibility.Visible;
        }

        private bool showPassword = false;

        private void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            // Placeholder
            if (string.IsNullOrWhiteSpace(tb.Text))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            string realPassword = tb.Name.Contains("Auth") ? realAuthPassword : realRegisterPassword;

            if (string.IsNullOrWhiteSpace(realPassword))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            string realPassword = tb.Name.Contains("Auth") ? realAuthPassword : realRegisterPassword;

            // Placeholder
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(tb.Text) && string.IsNullOrEmpty(realPassword)
                                             ? Visibility.Visible : Visibility.Collapsed;

            if (showPassword)
            {
                if (tb.Name.Contains("Auth")) realAuthPassword = tb.Text;
                else realRegisterPassword = tb.Text;
                return;
            }

            int selStart = tb.SelectionStart;

            if (tb.Text.Length < realPassword.Length)
            {
                int diff = realPassword.Length - tb.Text.Length;
                int removeIndex = Math.Max(0, selStart);
                int removeLength = Math.Min(diff, realPassword.Length - removeIndex);
                if (removeLength > 0)
                    realPassword = realPassword.Remove(removeIndex, removeLength);
            }
            else if (tb.Text.Length > realPassword.Length)
            {
                int diff = tb.Text.Length - realPassword.Length;
                int insertIndex = Math.Max(0, selStart - diff);

                if (insertIndex + diff <= tb.Text.Length && insertIndex >= 0)
                {
                    string added = tb.Text.Substring(insertIndex, diff);
                    realPassword = realPassword.Insert(insertIndex, added);
                }
            }

            // Сохраняем обратно в нужную переменную
            if (tb.Name.Contains("Auth")) realAuthPassword = realPassword;
            else realRegisterPassword = realPassword;

            // Заменяем ввод на точки
            tb.Text = new string('•', realPassword.Length);
            tb.SelectionStart = selStart;
        }

        // Глазик: показать пароль
        private void ShowPasswordButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            showPassword = true;

            var button = sender as Button;
            var grid = button.Parent as Grid;

            var tb = grid.Children.OfType<TextBox>().FirstOrDefault();
            if (tb == null) return;

            string realPassword = tb.Name.Contains("Auth") ? realAuthPassword : realRegisterPassword;

            tb.Text = realPassword;
            tb.SelectionStart = tb.Text.Length;
        }

        // Глазик: скрыть пароль
        private void ShowPasswordButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            showPassword = false;

            var button = sender as Button;
            var grid = button.Parent as Grid;

            var tb = grid.Children.OfType<TextBox>().FirstOrDefault();
            if (tb == null) return;

            string realPassword = tb.Name.Contains("Auth") ? realAuthPassword : realRegisterPassword;

            tb.Text = new string('•', realPassword.Length);
            tb.SelectionStart = tb.Text.Length;
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = tb.Tag.ToString();     // устанавливаем placeholder
                tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && tb.Text == tb.Tag.ToString())
            {
                tb.Text = "";
                tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e1e1e"));
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = tb.Tag.ToString();
                tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
            }
        }
    }
}
