using PianoTrainerApp.Models;
using PianoTrainerApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Главное окно приложения. Отвечает за навигацию между страницами,
    /// авторизацию/регистрацию пользователя, управление профилем,
    /// отображение статистики и обработку пользовательского ввода.
    /// </summary>
    public partial class MainWindow : Window
    {
        private User currentUser;

        private Dictionary<string, string> passwords = new Dictionary<string, string>();  // для хранения настоящих паролей textbox
        bool profileOpened = false;  // Открыта ли панель профиля
        private bool showPassword = false;

        public MainWindow()
        {
            InitializeComponent();
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
                    var libraryView = new LibraryView();
                    MainContent.Content = libraryView;

                    if (libraryView.DataContext is LibraryViewModel vm)
                    {
                        // Подписываемся на событие лайков
                        vm.FavoritesChanged += () => RefreshUserStats(includeStreak: false);
                    }

                    // подписка на событие завершения песни из PianoWindow
                    libraryView.SongFinishedInPiano += () => RefreshUserStats(includeStreak: false);

                    HighlightMenuItem(LibraryTextBlock);
                    break;

                case PageType.Lessons:
                    MainContent.Content = new LessonsView();
                    HighlightMenuItem(LessonsTextBlock);
                    break;
            }
        }

        // Обработчик клика по пунктам меню
        private void MenuTextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender == HomeTextBlock) ShowPage(PageType.Home);
            else if (sender == LibraryTextBlock) ShowPage(PageType.Library);
            else if (sender == LessonsTextBlock) ShowPage(PageType.Lessons);
        }
        
        // Подсветка выбранного пункта меню
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

        // Сброс подсветки всех пунктов меню
        private void ResetMenuItems()
        {
            ResetItem(HomeTextBlock);
            ResetItem(LessonsTextBlock);
            ResetItem(LibraryTextBlock);
        }

        // Сброс конкретного пункта меню
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

        // Обновление статистики пользователя
        private void RefreshUserStats(bool includeStreak = false)
        {
            if (currentUser == null) return;

            try
            {
                using (var db = new ReMinorContext())
                {
                    // Избранное
                    int favoritesCount = db.SongsUsers
                        .Count(su => su.UserId == currentUser.Id && su.IsFavorite);
                    FavouritesCountText.Text = favoritesCount.ToString();

                    // Получаем все записи пользователя
                    var userSongs = db.SongsUsers
                        .Where(su => su.UserId == currentUser.Id)
                        .ToList();

                    // Считаем количество сыгранных композиций
                    int tracksPlayedCount = userSongs
                        .Count(su => su.Accuracy >= 0 || su.BestAccuracy >= 0);
                    TracksCountText.Text = tracksPlayedCount.ToString();

                    // Считаем количество звёзд
                    int totalStars = userSongs
                        .Select(su =>
                        {
                            if (su.BestAccuracy < 50) return 0;
                            if (su.BestAccuracy < 70) return 1;
                            if (su.BestAccuracy < 90) return 2;
                            return 3;
                        })
                        .Sum();

                    StarsCountText.Text = totalStars.ToString();

                    // Стрик обновляем только при входе/автологине/регистрации
                    if (includeStreak)
                    {
                        var user = db.Users.FirstOrDefault(u => u.Id == currentUser.Id);
                        if (user != null)
                        {
                            StreakText.Text = user.CurrentStreak + " дн.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка обновления профиля: {ex.Message}");

                CustomMessageBox.Show("Ошибка обновления статистики в профиле", null, CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
        }

        // Кнопка выхода из приложения
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            var result = CustomMessageBox.Show(
                "Вы точно хотите выйти?",
                "Подтверждение выхода",
                CustomMessageBoxButton.YesNoCancel,
                CustomMessageBoxImage.Question
                );

            if (result == CustomMessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        // Открытие/закрытие панели профиля
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

        // Закрытие панели профиля по клику вне её
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

        // Переключение на форму регистрации
        private void RegisterTextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            // Скрываем панель авторизации
            AuthPanel.Visibility = Visibility.Collapsed;

            // Показываем панель регистрации
            RegisterPanel.Visibility = Visibility.Visible;
        }

        // Возврат к форме авторизации
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем панель регистрации
            RegisterPanel.Visibility = Visibility.Collapsed;

            // Показываем панель авторизации
            AuthPanel.Visibility = Visibility.Visible;
        }

        // Автоматический вход пользователя по сохранённому ID
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

                    // Обновляем дату последнего входа и стрик в БД
                    UpdateUserLoginStats(savedUserId);

                    // Показываем профиль
                    ShowUserProfile(user);
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка автологина: не удалось подключиться к базе данных: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла ошибка при автологине: не удалось подключиться к базе данных.", "Ошибка автологина", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка автологина: база данных недоступна: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла ошибка при автологине: не удалось подключиться к базе данных.", "Ошибка автологина", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Неизвестная ошибка при автологине: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла неизвестная ошибка при автологинеа.", "Ошибка автологина", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
        }

        // Отображение профиля пользователя
        private void ShowUserProfile(User user)
        {
            if (user == null) return;

            currentUser = user;

            // Обновляем UI: имя и email
            UsernameProfileText.Text = currentUser.Username;
            EmailProfileText.Text = currentUser.Email;

            // Показываем панель пользователя, скрываем регистрацию/авторизацию
            RegisterPanel.Visibility = Visibility.Collapsed;
            AuthPanel.Visibility = Visibility.Collapsed;
            UserEditPanel.Visibility = Visibility.Collapsed;
            UserPanel.Visibility = Visibility.Visible;

            // Сохраняем для автологина
            Properties.Settings.Default.CurrentUserId = currentUser.Id;
            Properties.Settings.Default.Save();

            // Обновляем статистику
            RefreshUserStats(includeStreak: true);
        }

        // Выход из аккаунта
        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = CustomMessageBox.Show(
                "Вы точно хотите выйти из аккаунта?",
                "Подтверждение выхода",
                CustomMessageBoxButton.YesNoCancel,
                CustomMessageBoxImage.Question
                );

            if (result == CustomMessageBoxResult.Yes)
            {
                Properties.Settings.Default.CurrentUserId = 0;
                Properties.Settings.Default.Save();

                currentUser = null;

                // Сбрасываем лайки в текущей библиотеке
                if (MainContent.Content is LibraryView libraryView &&
                    libraryView.DataContext is LibraryViewModel vm)
                {
                    vm.ResetSongs();
                }

                UserPanel.Visibility = Visibility.Collapsed;
                AuthPanel.Visibility = Visibility.Visible;
            }
        }

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

        // ---------- Инициализация placeholder для пароля ----------
        private void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            var placeholder = FindPlaceholder(tb);

            if (!passwords.ContainsKey(tb.Name) || string.IsNullOrEmpty(passwords[tb.Name]))
                placeholder.Visibility = Visibility.Visible;
        }

        // ---------- Обработка потери фокуса у поля пароля ----------
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            var placeholder = FindPlaceholder(tb);

            string realPassword = passwords.ContainsKey(tb.Name)
                    ? passwords[tb.Name]
                    : "";

            placeholder.Visibility = string.IsNullOrEmpty(realPassword)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // ---------- Обработка ввода пароля (кастомное скрытие символов) ----------
        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            if (!passwords.ContainsKey(tb.Name))
                passwords[tb.Name] = "";

            string realPassword = passwords[tb.Name];
            var placeholder = FindPlaceholder(tb);

            int selStart = tb.SelectionStart;

            // 👁 режим показа
            if (showPassword)
            {
                passwords[tb.Name] = tb.Text;

                placeholder.Visibility = string.IsNullOrEmpty(tb.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                return;
            }

            // удаление
            if (tb.Text.Length < realPassword.Length)
            {
                int diff = realPassword.Length - tb.Text.Length;
                int removeIndex = Math.Max(0, selStart);
                int removeLength = Math.Min(diff, realPassword.Length - removeIndex);

                if (removeLength > 0)
                    realPassword = realPassword.Remove(removeIndex, removeLength);
            }
            // добавление
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

            passwords[tb.Name] = realPassword;

            placeholder.Visibility = string.IsNullOrEmpty(realPassword)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

            tb.Text = new string('•', realPassword.Length);
            tb.SelectionStart = selStart;
        }

        // ---------- Обработка вставки в поле пароля ----------
        private void PasswordBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = e.DataObject.GetData(DataFormats.Text) as string ?? "";

            if (!passwords.ContainsKey(tb.Name))
                passwords[tb.Name] = "";

            string realPassword = passwords[tb.Name];

            int selectionStart = tb.SelectionStart;
            int selectionLength = tb.SelectionLength;

            // Вставляем в реальный пароль
            realPassword = realPassword.Remove(selectionStart, selectionLength);
            realPassword = realPassword.Insert(selectionStart, pastedText);

            passwords[tb.Name] = realPassword;

            // Отменяем стандартную вставку
            e.CancelCommand();

            // Обновляем UI
            tb.Text = new string('•', realPassword.Length);
            tb.SelectionStart = selectionStart + pastedText.Length;

            var placeholder = FindPlaceholder(tb);
            placeholder.Visibility = string.IsNullOrEmpty(realPassword)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // ---------- Показ пароля при зажатии глазика ----------
        private void ShowPasswordButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            showPassword = true;

            var button = sender as Button;
            var grid = button.Parent as Grid;
            var tb = grid.Children.OfType<TextBox>().FirstOrDefault();
            if (tb == null) return;

            string realPassword = passwords.ContainsKey(tb.Name)
                    ? passwords[tb.Name]
                    : "";

            tb.Text = realPassword;
            tb.SelectionStart = tb.Text.Length;
        }

        // ---------- Скрытие пароля при отпускании глазика ----------
        private void ShowPasswordButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            showPassword = false;

            var button = sender as Button;
            var grid = button.Parent as Grid;
            var tb = grid.Children.OfType<TextBox>().FirstOrDefault();
            if (tb == null) return;

            string realPassword = passwords.ContainsKey(tb.Name)
                    ? passwords[tb.Name]
                    : "";

            tb.Text = new string('•', realPassword.Length);
            tb.SelectionStart = tb.Text.Length;
        }

        // Поиск placeholder внутри TextBox
        private TextBlock FindPlaceholder(TextBox tb)
        {
            var grid = tb.Parent as Grid;

            return grid?.Children
                .OfType<TextBlock>()
                .FirstOrDefault(t => t.Name.Contains("Placeholder"));
        }

        // Проверка логина
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
            if (!char.IsLetter(username[0]))
            {
                error = "Логин должен начинаться с буквы";
                return false;
            }

            return true;
        }
        
        // Проверка email
        bool IsValidEmail(string email)
        {
            string pattern = @"^(?=.{1,254}$)(?=.{1,64}@)([a-zA-Z0-9_-]+(?:\.[a-zA-Z0-9_-]+)*)@([a-zA-Z]{2,64}(?:\.[a-zA-Z]{2,64})+)$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        // Проверка пароля
        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (password.Length > 32) return false;
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
                // Получаем пароль из словаря
                passwords.TryGetValue("RegisterPasswordBox", out string password);

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    CustomMessageBox.Show("Все поля должны быть заполнены", "Ошибка регистрации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                    return;
                }

                if (!IsValidUsername(username, out string errorUsername))
                {
                    CustomMessageBox.Show(errorUsername, "Ошибка регистрации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                    return;
                }

                if (!IsValidEmail(email))
                {
                    CustomMessageBox.Show("Некорректный формат e-mail.", "Ошибка регистрации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                    return;
                }

                if (!IsValidPassword(password))
                {
                    CustomMessageBox.Show("Пароль не должен быть более 32 символов, а также состоять из одного пробела", "Ошибка регистрации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                    return;
                }

                string passwordHash = PasswordHelper.HashPassword(password);

                try
                {
                    using (var db = new ReMinorContext())
                    {
                        if (db.Users.Any(u => u.Username == username))
                        {
                            CustomMessageBox.Show("Пользователь с таким логином уже существует", "Ошибка регистрации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
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
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка регистрации: не удалось подключиться к базе данных: {ex.InnerException.Message}");
                    CustomMessageBox.Show("Возникла ошибка при попытке регистрации: не удалось подключиться к базе данных.", "Ошибка регистрации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                }
                catch (System.Data.Entity.Core.EntityException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка регистрации: база данных недоступна: {ex.InnerException.Message}");
                    CustomMessageBox.Show("Возникла ошибка при попытке регистрации: не удалось подключиться к базе данных.", "Ошибка регистрации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Неизвестная ошибка при регистрации: {ex.InnerException.Message}");
                    CustomMessageBox.Show("Возникла неизвестная ошибка при регистрации.", "Ошибка регистрации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                }

                CustomMessageBox.Show("Регистрация прошла успешно!", "Добро пожаловать!", CustomMessageBoxButton.OK, CustomMessageBoxImage.Success);

                // Обновляем лайки в библиотеке
                if (MainContent.Content is LibraryView libraryView && libraryView.DataContext is LibraryViewModel vm)
                {
                    vm.SetCurrentUser(currentUser.Id);
                }

                // Обновляем дату последнего входа и стрик в БД
                UpdateUserLoginStats(currentUser.Id);

                // Показываем профиль
                ShowUserProfile(currentUser);
            }


            else if (button.Name == "AuthButton")
            {
                string username = AuthUsernameTextBox.Text.Trim();
                // Получаем пароль из словаря
                passwords.TryGetValue("AuthPasswordBox", out string password);

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    CustomMessageBox.Show("Введите логин и пароль", "Ошибка авторизации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
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
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка авторизации: не удалось подключиться к базе данных: {ex.InnerException.Message}");
                    CustomMessageBox.Show("Возникла ошибка при попытке авторизации: не удалось подключиться к базе данных.", "Ошибка авторизации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                }
                catch (System.Data.Entity.Core.EntityException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка авторизации: база данных недоступна: {ex.InnerException.Message}");
                    CustomMessageBox.Show("Возникла ошибка при попытке авторизации: не удалось подключиться к базе данных.", "Ошибка авторизации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Неизвестная ошибка при авторизации: {ex.InnerException.Message}");
                    CustomMessageBox.Show("Возникла неизвестная ошибка при попытке авторизации.", "Ошибка авторизации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                }

                if (currentUser == null)
                {
                    CustomMessageBox.Show("Неверный логин или пароль", "Ошибка авторизации", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                    return;
                }

                CustomMessageBox.Show($"Рады видеть вас снова, {currentUser.Username}!\nГотовы улучшать своё мастерство?)", "Добро пожаловать!", CustomMessageBoxButton.OK, CustomMessageBoxImage.Success);

                // Обновляем лайки в библиотеке и статистику пользователя
                if (MainContent.Content is LibraryView libraryView && libraryView.DataContext is LibraryViewModel vm)
                {
                    vm.SetCurrentUser(currentUser.Id);
                }

                // Обновляем дату последнего входа и стрик в БД
                UpdateUserLoginStats(currentUser.Id);

                // Показываем профиль
                ShowUserProfile(currentUser);
            }
        }

        // Обновление даты последнего входа и стрика в БД
        private void UpdateUserLoginStats(int userId)
        {
            try
            {
                using (var db = new ReMinorContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);
                    if (user == null) return;

                    // тут обновляем LastLoginDate и CurrentStreak
                    var today = DateTime.Today;
                    if (user.LastLoginDate.HasValue)
                    {
                        var daysDiff = (today - user.LastLoginDate.Value.Date).Days;

                        if (daysDiff == 1)
                            user.CurrentStreak += 1;
                        else if (daysDiff > 1)
                            user.CurrentStreak = 1;
                    }
                    else
                    {
                        user.CurrentStreak = 1;
                    }

                    user.LastLoginDate = today;
                    db.SaveChanges();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка обновления сеансов пользователя (SQL): {ex.Message}");
                CustomMessageBox.Show(
                    "Не удалось обновить сеансы пользователя: ошибка подключения к базе данных.",
                    "Ошибка обновления сеансов",
                    CustomMessageBoxButton.OK,
                    CustomMessageBoxImage.Error
                );
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка обновления сеансов пользователя (Entity): {ex.Message}");

                CustomMessageBox.Show(
                    "Не удалось обновить сеансы пользователя: база данных недоступна.",
                    "Ошибка обновления сеансов",
                    CustomMessageBoxButton.OK,
                    CustomMessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Неизвестная ошибка при обновлении сеансов пользователя: {ex.Message}");

                CustomMessageBox.Show(
                    "Не удалось обновить сеансы пользователя: произошла неизвестная ошибка.",
                    "Ошибка обновления сеансов",
                    CustomMessageBoxButton.OK,
                    CustomMessageBoxImage.Error
                );
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
            if (currentUser == null) return;

            string newUsername = EditUsernameBox.Text;
            string newEmail = EditEmailBox.Text;

            // Проверки
            if (string.IsNullOrWhiteSpace(newUsername) || string.IsNullOrWhiteSpace(newEmail))
            {
                CustomMessageBox.Show("Все поля должны быть заполнены", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                return;
            }

            if (!IsValidUsername(newUsername, out string usernameError))
            {
                CustomMessageBox.Show(usernameError, "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(newEmail))
            {
                CustomMessageBox.Show("Некорректный формат e-mail", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                return;
            }

            // Подтверждение действия
            var result = CustomMessageBox.Show("Сохранить изменения профиля?", "Подтверждение", CustomMessageBoxButton.YesNoCancel, CustomMessageBoxImage.Question);
            if (result != CustomMessageBoxResult.Yes) return;

            try
            {
                using (var db = new ReMinorContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == currentUser.Id);
                    if (user == null)
                    {
                        CustomMessageBox.Show("Не удалось сохранить изменения профиля: пользователь не найден в БД", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                        return;
                    }

                    // Проверка уникальности логина
                    if (db.Users.Any(u => u.Username == newUsername && u.Id != currentUser.Id))
                    {
                        CustomMessageBox.Show("Пользователь с таким логином уже существует", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                        return;
                    }

                    user.Username = newUsername;
                    user.Email = newEmail;
                    db.SaveChanges();

                    // Обновляем текущего пользователя
                    currentUser.Username = newUsername;
                    currentUser.Email = newEmail;

                    CustomMessageBox.Show("Профиль успешно обновлён", null, CustomMessageBoxButton.OK, CustomMessageBoxImage.Info);

                    // Обновляем отображение
                    ShowUserProfile(currentUser);
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка сохранения изменений профиля: не удалось подключиться к базе данных: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла ошибка при сохранении изменений профиля: не удалось подключиться к базе данных.", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка сохранения изменений профиля: база данных недоступна: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла ошибка при сохранении изменений профиля: не удалось подключиться к базе данных.", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Неизвестная ошибка при сохранении изменений профиля: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла неизвестная ошибка при сохранении изменений профиля.", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
        }

        // Обработчик кнопки сохранения изменененного пароля пользователя
        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;

            // Получаем пароли из словаря
            passwords.TryGetValue("CurrentPasswordBox", out string currentPassword);
            passwords.TryGetValue("NewPasswordBox", out string newPassword);
            passwords.TryGetValue("RepeatPasswordBox", out string repeatPassword);

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(repeatPassword))
            {
                CustomMessageBox.Show("Заполните все поля пароля", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                return;
            }

            // Проверка нового пароля
            if (!IsValidPassword(newPassword))
            {
                CustomMessageBox.Show(
                    "Новый пароль не должен быть более 32 символов, а также состоять из одного пробела",
                    "Ошибка сохранения изменений",
                    CustomMessageBoxButton.OK,
                    CustomMessageBoxImage.Warning
                    );
                return;
            }

            if (newPassword != repeatPassword)
            {
                CustomMessageBox.Show(
                    "Новый пароль и подтверждение не совпадают",
                    "Ошибка сохранения изменений",
                    CustomMessageBoxButton.OK,
                    CustomMessageBoxImage.Warning
                    );
                return;
            }

            // Подтверждение действия
            var result = CustomMessageBox.Show("Сменить пароль?", "Подтверждение", CustomMessageBoxButton.YesNoCancel, CustomMessageBoxImage.Question);
            if (result != CustomMessageBoxResult.Yes) return;

            try
            {
                using (var db = new ReMinorContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == currentUser.Id);
                    if (user == null)
                    {
                        CustomMessageBox.Show("Не удалось сохранить изменения пароля: пользователь не найден в БД", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                        return;
                    }

                    string currentPasswordHash = PasswordHelper.HashPassword(currentPassword);
                    if (user.PasswordHash != currentPasswordHash)
                    {
                        CustomMessageBox.Show("Не удалось сохранить изменения пароля: текущий пароль введён неверно", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                        return;
                    }

                    user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                    db.SaveChanges();

                    // Обновляем словарь и переменные
                    passwords["CurrentPasswordBox"] = "";
                    passwords["NewPasswordBox"] = "";
                    passwords["RepeatPasswordBox"] = "";

                    CustomMessageBox.Show("Пароль успешно изменён", null, CustomMessageBoxButton.OK, CustomMessageBoxImage.Info);
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка сохранения пароля: не удалось подключиться к базе данных: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла ошибка при сохранении изменений пароля: не удалось подключиться к базе данных.", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка сохранения пароля: база данных недоступна: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла ошибка при сохранении изменений пароля: не удалось подключиться к базе данных.", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Неизвестная ошибка при сохранении пароля: {ex.InnerException.Message}");
                CustomMessageBox.Show("Возникла неизвестная ошибка при сохранении изменений пароля.", "Ошибка сохранения изменений", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
        }
    }
}

