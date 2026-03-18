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

        // ---------- Универсальный TextChanged для логина/почты ----------
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            // placeholder — ищем как TextBlock в родительском Grid
            var parentGrid = tb.Parent as Grid;
            if (parentGrid == null) return;

            // ищем TextBlock с IsHitTestVisible=false (наш placeholder)
            var placeholder = parentGrid.Children.OfType<TextBlock>()
                                 .FirstOrDefault(t => !t.IsHitTestVisible);
            if (placeholder == null) return;

            // скрываем placeholder, если есть текст
            placeholder.Visibility = string.IsNullOrEmpty(tb.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        // ---------- Универсальный Loaded для пароля ----------
        private void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            var placeholder = tb.Name.Contains("Auth") ? AuthPasswordPlaceholder : RegisterPasswordPlaceholder;

            if (string.IsNullOrWhiteSpace(tb.Text))
                placeholder.Visibility = Visibility.Visible;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            bool isAuth = tb.Name.Contains("Auth");

            var placeholder = isAuth ? AuthPasswordPlaceholder : RegisterPasswordPlaceholder;
            string realPassword = isAuth ? realAuthPassword : realRegisterPassword;

            placeholder.Visibility = string.IsNullOrEmpty(realPassword)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        // ---------- Универсальный TextChanged для пароля ----------
        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            bool isAuth = tb.Name.Contains("Auth");

            var placeholder = isAuth ? AuthPasswordPlaceholder : RegisterPasswordPlaceholder;
            string realPassword = isAuth ? realAuthPassword : realRegisterPassword;

            int selStart = tb.SelectionStart;

            // 👉 РЕЖИМ ПОКАЗА ПАРОЛЯ
            if (showPassword)
            {
                realPassword = tb.Text;

                if (isAuth) realAuthPassword = realPassword;
                else realRegisterPassword = realPassword;

                placeholder.Visibility = string.IsNullOrEmpty(realPassword)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                return;
            }

            // 👉 ОБНОВЛЕНИЕ realPassword
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

            // 👉 СОХРАНЯЕМ
            if (isAuth) realAuthPassword = realPassword;
            else realRegisterPassword = realPassword;

            // 💥 ВАЖНО: placeholder только от realPassword
            placeholder.Visibility = string.IsNullOrEmpty(realPassword)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // 👉 ВСЕГДА маска
            tb.Text = new string('•', realPassword.Length);
            tb.SelectionStart = selStart;
        }
        // ---------- Универсальный глазик ----------
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

        // ---------- Проверка логина ----------
        private bool IsValidUsername(string username)
        {
            if (username.Length < 2) return false;
            if (!char.IsLetter(username[0])) return false;
            return true;
        }

        // ---------- Проверка пароля ----------
        private bool IsValidPassword(string password)
        {
            if (password.Length < 6) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            return true;
        }

        // ---------- Кнопка входа / регистрации ----------
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            using (var db = new ReMinorContext())
            {
                if (button.Name == "RegisterButton")
                {
                    string username = RegisterUsernameTextBox.Text.Trim();
                    string email = RegisterEmailTextBox.Text.Trim();
                    string password = realRegisterPassword;

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    {
                        MessageBox.Show("Все поля должны быть заполнены");
                        return;
                    }

                    if (!IsValidUsername(username))
                    {
                        MessageBox.Show("Логин должен быть минимум 2 символа и начинаться с буквы");
                        return;
                    }

                    if (db.Users.Any(u => u.Username == username))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует");
                        return;
                    }

                    if (!IsValidPassword(password))
                    {
                        MessageBox.Show("Пароль должен быть минимум 6 символов, содержать заглавную букву, строчную и цифру");
                        return;
                    }

                    string passwordHash = PasswordHelper.HashPassword(password);

                    currentUser = new User { Username = username, Email = email, PasswordHash = passwordHash };
                    db.Users.Add(currentUser);
                    db.SaveChanges();
                }
                else if (button.Name == "AuthButton")
                {
                    string username = AuthUsernameTextBox.Text.Trim();
                    string password = realAuthPassword;

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        MessageBox.Show("Введите логин и пароль");
                        return;
                    }

                    string passwordHash = PasswordHelper.HashPassword(password);
                    currentUser = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);

                    if (currentUser == null)
                    {
                        MessageBox.Show("Неверный логин или пароль");
                        return;
                    }
                }
            }

            // ---------- АВТОЛОГИН ----------
            Properties.Settings.Default.CurrentUserId = currentUser.Id;
            Properties.Settings.Default.Save();

            // ---------- Показ профиля ----------
            UsernameProfileText.Text = currentUser.Username;
            RegisterPanel.Visibility = Visibility.Collapsed;
            AuthPanel.Visibility = Visibility.Collapsed;
            UserPanel.Visibility = Visibility.Visible;
        }
    }
}
