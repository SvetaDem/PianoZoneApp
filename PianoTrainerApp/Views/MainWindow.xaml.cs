using PianoTrainerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
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
            //ShowHome();
            ShowPage(PageType.Home);
            AutoLogin();
        }

        // Универсальный метод для смены страницы
        private void ShowPage(PageType page)
        {
            // Сбрасываем все подсветки
            ResetMenuItems();

            // Переключаем контент и подсвечиваем нужный TextBlock
            switch (page)
            {
                case PageType.Home:
                    var home = new HomeView();
                    home.NavigateRequested += ShowPage; // подписка на событие

                    MainContent.Content = home;
                    HighlightMenuItem(HomeTextBlock);
                    break;

                case PageType.Library:
                    MainContent.Content = new LibraryView();
                    HighlightMenuItem(LibraryTextBlock);
                    break;

                case PageType.Beginner:
                    //MainContent.Content = new BeginnerView();
                    HighlightMenuItem(BeginnerTextBlock);
                    break;
            }
        }

        // В родительском окне (например, MainWindow.xaml.cs) подписываемся на событие при создании HomeView
        /*        private void ShowHome()
                {
                    var home = new HomeView();

                    // Подписываемся на события навигации
                    home.NavigateRequested += page =>
                    {
                        ShowPage(page); // Используем универсальный метод из предыдущего шага
                    };

                    MainContent.Content = home;

                    // Обновляем подсветку меню
                    ResetMenuItems();
                    HighlightMenuItem(HomeTextBlock);
                }
        */
        // Сброс выделения всех пунктов
        private void ResetMenuItems()
        {
            ResetItem(HomeTextBlock);
            ResetItem(BeginnerTextBlock);
            ResetItem(LibraryTextBlock);
        }

        private void ResetItem(TextBlock tb)
        {
            tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));

            var scale = tb.RenderTransform as ScaleTransform;
            if (scale != null)
            {
                scale.ScaleX = 1;
                scale.ScaleY = 1;
            }
        }

        // Подсветка выбранного пункта
        private void HighlightMenuItem(TextBlock tb)
        {
            if (tb == null) return;

            tb.Foreground = Brushes.White;

            var scale = tb.RenderTransform as ScaleTransform;
            if (scale != null)
            {
                scale.ScaleX = 1.2;
                scale.ScaleY = 1.2;
            }
        }

        private void MenuTextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender == HomeTextBlock) ShowPage(PageType.Home);
            else if (sender == LibraryTextBlock) ShowPage(PageType.Library);
            else if (sender == BeginnerTextBlock) ShowPage(PageType.Beginner);
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
            if (savedUserId == 0) return;

            try
            {
                using (var db = new ReMinorContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == savedUserId);

                    if (user == null)
                        return;

                    ShowUserProfile(user);
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                MessageBox.Show("Ошибка SQL при автологине: " + ex.InnerException.Message);
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Неизвестная ошибка при автологине: " + ex.InnerException.Message);
            }
        }

        private void ShowUserProfile(User user)
        {
            if (user == null) return;

            currentUser = user;

            UsernameProfileText.Text = currentUser.Username;
            EmailProfileText.Text = currentUser.Email;
            try
            {
                using (var db = new ReMinorContext())
                {
                    int favoritesCount = db.SongsUsers
                        .Count(su => su.UserId == currentUser.Id && su.IsFavorite);

                    FavoritesCountText.Text = $"⭐ {favoritesCount}";
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                MessageBox.Show("Ошибка SQL при автологине: " + ex.InnerException.Message);
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Неизвестная ошибка при автологине: " + ex.InnerException.Message);
            }

            RegisterPanel.Visibility = Visibility.Collapsed;
            AuthPanel.Visibility = Visibility.Collapsed;
            UserPanel.Visibility = Visibility.Visible;

            // Сохраняем для автологина
            Properties.Settings.Default.CurrentUserId = currentUser.Id;
            Properties.Settings.Default.Save();
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
        private bool IsValidUsername(string username, out string error)
        {
            error = "";

            string allowedSpecials = "_-.";

            // Проверяем на недопустимые символы
            if (!username.All(c => char.IsLetterOrDigit(c) || allowedSpecials.Contains(c)))
            {
                error = "Логин может содержать только буквы, цифры и символы: _ . -";
                return false;
            }

            // Проверка: спецсимволы не стоят рядом
            for (int i = 1; i < username.Length; i++)
            {
                if (allowedSpecials.Contains(username[i]) && allowedSpecials.Contains(username[i - 1]))
                {
                    error = "Спецсимволы в логине не могут идти подряд";
                    return false;
                }
            }

            // минимальная длина
            if (username.Length < 3)
            {
                error = "Логин должен быть не менее 3 символов";  // цифры, буквы 
                return false;
            }

            // максимальная длина
            if (username.Length > 32)
            {
                error = "Логин должен быть не более 32 символов";  // цифры, буквы 
                return false;
            }


            // первая буква
            if (!char.IsLetter(username[0]) || !char.IsLetter(username[username.Length - 1]))
            {
                error = "Логин должен начинаться и заканчиваться буквой";
                return false;
            }

            return true;
        }
        // ---------- Проверка email ----------
        bool IsValidEmail(string email)
        {
            string pattern = @"^(?=.{1,254}$)(?=.{1,64}@)([a-zA-Z0-9_-]+(?:\.[a-zA-Z0-9_-]+)*)@([a-zA-Z]{2,64}(?:\.[a-zA-Z]{2,64})+)$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        // ---------- Проверка пароля ----------
        private bool IsValidPassword(string password)
        {
            if (password.Length < 6 || password.Length > 32) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            return true;
        }

        // Обработчик кнопки авторизации/регистрации
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            if (button.Name == "RegisterButton")
            {
                string username = RegisterUsernameTextBox.Text;
                string email = RegisterEmailTextBox.Text.Trim();
                string password = realRegisterPassword;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Все поля должны быть заполнены");
                    return;
                }

                if (!IsValidUsername(username, out string errorUsername))
                {
                    MessageBox.Show(errorUsername, "Ошибка в логине");
                    return;
                }

                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Ошибка в e-mail");
                    return;
                }

                if (!IsValidPassword(password))
                {
                    MessageBox.Show("Пароль должен быть от 6 до 32 символов, содержать заглавную букву, строчную и цифру", "Ошибка в пароле");
                    return;
                }

                string passwordHash = PasswordHelper.HashPassword(password);

                try
                {
                    using (var db = new ReMinorContext())
                    {
                        if (db.Users.Any(u => u.Username == username))
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует");
                            return;
                        }

                        currentUser = new User
                        {
                            Username = username,
                            Email = email,
                            PasswordHash = passwordHash
                        };

                        db.Users.Add(currentUser);
                        db.SaveChanges();
                    }
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    MessageBox.Show("Ошибка SQL: " + ex.InnerException.Message);
                    return;
                }
                catch (System.Data.Entity.Core.EntityException ex)
                {
                    MessageBox.Show("Ошибка подключения к БД: " + ex.InnerException.Message);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Неизвестная ошибка: " + ex.InnerException.Message);
                    return;
                }

                ShowUserProfile(currentUser);
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
                try
                {
                    using (var db = new ReMinorContext())
                    {
                        currentUser = db.Users
                            .FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);
                    }
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    MessageBox.Show("Ошибка SQL: " + ex.InnerException.Message);
                    return;
                }
                catch (System.Data.Entity.Core.EntityException ex)
                {
                    MessageBox.Show("Ошибка подключения к БД: " + ex.InnerException.Message);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Неизвестная ошибка: " + ex.InnerException.Message);
                    return;
                }

                if (currentUser == null)
                {
                    MessageBox.Show("Неверный логин или пароль");
                    return;
                }

                // Показ профиля
                ShowUserProfile(currentUser);
            }
        }

        // Обработчик кнопки возврата в профиль после редактирования
        private void BackToProfile_Click(object sender, RoutedEventArgs e)
        {
            UserEditPanel.Visibility = Visibility.Collapsed;
            UserPanel.Visibility = Visibility.Visible;
        }

        // Обработчик кнопки редактирования профиля
        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            EditUsernameBox.Text = currentUser.Username;
            EditEmailBox.Text = currentUser.Email;

            UserEditPanel.Visibility = Visibility.Visible;
            UserPanel.Visibility = Visibility.Collapsed;
        }

        // Обработчик кнопки сохранения изменений данных пользователя
        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
/*            try
            {
                using (var db = new ReMinorContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == currentUser.Id);

                    user.Username = EditUsernameBox.Text.Trim();
                    user.Email = EditEmailBox.Text.Trim();

                    // если меняли пароль
                    if (PasswordPanel.Visibility == Visibility.Visible)
                    {
                        string currentHash = PasswordHelper.HashPassword(CurrentPasswordBox.Password);

                        if (user.PasswordHash != currentHash)
                        {
                            MessageBox.Show("Неверный текущий пароль");
                            return;
                        }

                        if (NewPasswordBox.Password != RepeatPasswordBox.Password)
                        {
                            MessageBox.Show("Пароли не совпадают");
                            return;
                        }

                        user.PasswordHash = PasswordHelper.HashPassword(NewPasswordBox.Password);
                    }

                    db.SaveChanges();

                    currentUser = user;
                }

                MessageBox.Show("Сохранено ✨");
                BackToProfile_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
*/        }

        // Обработчик кнопки сохранения изменененного пароля пользователя
        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            //
        }
    }
}

