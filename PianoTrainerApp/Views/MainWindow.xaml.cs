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
            if (button == null) return;

            using (var db = new ReMinorContext())
            {
                if (button.Name == "RegisterButton")
                {
                    string username = RegisterUsernameTextBox.Text.Trim();
                    string email = RegisterEmailTextBox.Text.Trim();
                    string password = realRegisterPassword;

                    // проверка пустых полей
                    if (string.IsNullOrWhiteSpace(username) ||
                        string.IsNullOrWhiteSpace(email) ||
                        string.IsNullOrWhiteSpace(password))
                    {
                        MessageBox.Show("Все поля должны быть заполнены");
                        return;
                    }

                    // проверка уникальности логина
                    if (db.Users.Any(u => u.Username == username))
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
                else if (button.Name == "AuthButton")
                {
                    string username = AuthUsernameTextBox.Text.Trim();
                    string password = realAuthPassword;

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

        // ---------------- PLACEHOLDER ЛОГИН ----------------
        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UsernamePlaceholder.Visibility = string.IsNullOrEmpty(AuthUsernameTextBox.Text)
                                             ? Visibility.Visible
                                             : Visibility.Collapsed;
        }

        // ---------------- PLACEHOLDER ПАРОЛЬ ----------------
        private void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AuthPasswordBox.Text))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(realAuthPassword))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        // ---------------- МАСКА ПАРОЛЯ ----------------
        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(tb.Text) && string.IsNullOrEmpty(realAuthPassword)
                                             ? Visibility.Visible
                                             : Visibility.Collapsed;

            if (showPassword)
            {
                realAuthPassword = tb.Text;
                return;
            }

            int selStart = tb.SelectionStart;

            if (tb.Text.Length < realAuthPassword.Length)
            {
                int diff = realAuthPassword.Length - tb.Text.Length;
                int removeIndex = Math.Max(0, selStart);
                int removeLength = Math.Min(diff, realAuthPassword.Length - removeIndex);
                if (removeLength > 0)
                    realAuthPassword = realAuthPassword.Remove(removeIndex, removeLength);
            }
            else if (tb.Text.Length > realAuthPassword.Length)
            {
                int diff = tb.Text.Length - realAuthPassword.Length;
                int insertIndex = Math.Max(0, selStart - diff);
                if (insertIndex + diff <= tb.Text.Length && insertIndex >= 0)
                {
                    string added = tb.Text.Substring(insertIndex, diff);
                    realAuthPassword = realAuthPassword.Insert(insertIndex, added);
                }
            }

            tb.Text = new string('•', realAuthPassword.Length);
            tb.SelectionStart = selStart;
        }

        // ---------------- ГЛАЗИК ----------------
        private void ShowPasswordButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            showPassword = true;
            AuthPasswordBox.Text = realAuthPassword;
            AuthPasswordBox.SelectionStart = AuthPasswordBox.Text.Length;
        }

        private void ShowPasswordButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            showPassword = false;
            AuthPasswordBox.Text = new string('•', realAuthPassword.Length);
            AuthPasswordBox.SelectionStart = AuthPasswordBox.Text.Length;
        }
    }
}
