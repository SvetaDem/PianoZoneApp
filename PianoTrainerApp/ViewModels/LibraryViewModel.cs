using PianoTrainerApp.Models;
using PianoTrainerApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;  // для новой версии Include с лямбдой
using System.Linq; // для Cast и Select
using System.Runtime.CompilerServices;
using System.Windows;

namespace PianoTrainerApp.ViewModels
{
    /// <summary>
    /// ViewModel для страницы библиотеки песен.
    /// Управляет загрузкой, фильтрацией и отображением песен,
    /// избранными песнями пользователя и поиском.
    /// </summary>
    public class LibraryViewModel : INotifyPropertyChanged
    {
        // ---------------------------
        // Песни
        // ---------------------------
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
        public ObservableCollection<Song> FilteredSongs { get; set; } = new ObservableCollection<Song>();

        // ---------------------------
        // Жанры
        // ---------------------------
        // Список всех жанров + специальный "ничего не выбрано"
        public ObservableCollection<string> Genres { get; set; } = new ObservableCollection<string>();

        private string selectedGenre;
        public string SelectedGenre
        {
            get => selectedGenre;
            set
            {
                selectedGenre = value;
                OnPropertyChanged();
                FilterSongs();
            }
        }

        // ---------------------------
        // Поиск
        // ---------------------------
        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                OnPropertyChanged();
                FilterSongs();
            }
        }

        // ---------------------------
        // Пользователь и избранное
        // ---------------------------
        public bool IsUserAuthorized => CurrentUserId != 0;
        public int FavoritesCount => Songs.Count(s => s.IsFavorite);

        // Для отображения списка понравившихся песен
        private bool showingFavorites = false;

        public int CurrentUserId { get; set; }
        public event Action FavoritesChanged;

        // ---------------------------
        // Выбранная песня
        // ---------------------------
        private Song selectedSong;
        public Song SelectedSong
        {
            get => selectedSong;
            set { selectedSong = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Инициализирует ViewModel и загружает песни.
        /// </summary>
        public LibraryViewModel()
        {
            CurrentUserId = Properties.Settings.Default.CurrentUserId;
            LoadSongs();
        }

        // ---------------------------
        // Методы для загрузки и сброса песен
        // ---------------------------
        /// <summary>
        /// Загружает песни и жанры из базы данных, обновляет информацию об избранном пользователя.
        /// </summary>
        public void LoadSongs()
        {
            try
            {
                using (var db = new ReMinorContext())
                {
                    var songsFromDb = db.Songs
                        .Include(s => s.Genres)
                        .Include(s => s.SongUsers)
                        .ToList();

                    Songs = new ObservableCollection<Song>(songsFromDb);

                    var allGenres = db.Genres
                        .Select(g => g.GenreName)
                        .OrderBy(x => x)
                        .ToList();

                    Genres.Add("Все жанры");
                    foreach (var g in allGenres)
                        Genres.Add(g);

                    if (CurrentUserId != 0)
                    {
                        // Выставляем лайки пользователя
                        foreach (var song in Songs)
                        {
                            var songUser = song.SongUsers?
                        .FirstOrDefault(su => su.UserId == CurrentUserId);

                            // лайк
                            song.IsFavorite = songUser?.IsFavorite ?? false;

                            // точность и звёзды
                            song.CurrentAccuracy = songUser?.Accuracy ?? 0;
                            song.BestAccuracy = songUser?.BestAccuracy ?? 0;
                        }
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка загрузки песен: не удалось подключиться к базе данных: {ex.InnerException.Message}");
                CustomMessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка загрузки песен", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка загрузки песен: база данных недоступна: {ex.InnerException.Message}");
                CustomMessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка загрузки песен", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Неизвестная ошибка при загрузке песен: {ex.InnerException.Message}");
                CustomMessageBox.Show("Неизвестная ошибка.", "Ошибка загрузки песен", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }

            FilteredSongs = new ObservableCollection<Song>(Songs);
            SelectedGenre = "Все жанры";

            OnPropertyChanged(nameof(FilteredSongs));
            OnPropertyChanged(nameof(Genres));
        }

        /// <summary>
        /// Сбрасывает избранные песни и текущего пользователя (например, при logout).
        /// </summary>
        public void ResetSongs()
        {
            CurrentUserId = 0;
            Properties.Settings.Default.CurrentUserId = 0;
            Properties.Settings.Default.Save();

            foreach (var song in Songs)
            {
                song.IsFavorite = false;
                song.CurrentAccuracy = 0;
                song.BestAccuracy = 0;
            }

            FilteredSongs = new ObservableCollection<Song>(Songs);
            SelectedGenre = "Все жанры";

            OnPropertyChanged(nameof(FilteredSongs));
            OnPropertyChanged(nameof(Genres));
        }

        /// <summary>
        /// Обновляет идентификатор текущего пользователя и загружает песни.
        /// </summary>
        public void SetCurrentUser(int userId)
        {
            CurrentUserId = userId;
            LoadSongs();
        }

        // ---------------------------
        // Методы для работы с избранным
        // ---------------------------
        /// <summary>
        /// Переключает состояние "избранная песня" для заданной песни.
        /// </summary>
        public void ToggleFavorite(Song song)
        {
            if (song == null) return;

            try
            {
                using (var db = new ReMinorContext())
                {
                    var songUser = db.SongsUsers
                        .FirstOrDefault(su => su.SongId == song.Id && su.UserId == CurrentUserId);

                    if (songUser == null)
                    {
                        // создаем новую запись
                        songUser = new SongUser
                        {
                            SongId = song.Id,
                            UserId = CurrentUserId,
                            IsFavorite = true
                        };

                        db.SongsUsers.Add(songUser);
                        song.IsFavorite = true;
                    }
                    else
                    {
                        // переключаем лайк
                        songUser.IsFavorite = !songUser.IsFavorite;
                        song.IsFavorite = songUser.IsFavorite;
                    }

                    db.SaveChanges();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка отметки любимой песни: не удалось подключиться к базе данных: {ex.InnerException.Message}");
                CustomMessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка отметки любимой песни", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка отметки любимой песни: база данных недоступна: {ex.InnerException.Message}");
                CustomMessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка отметки любимой песни", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Неизвестная ошибка при отметке любимой песнин: {ex.InnerException.Message}");
                CustomMessageBox.Show("Неизвестная ошибка.", "Ошибка отметки любимой песни", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
            }

            // вызов события
            FavoritesChanged?.Invoke();

            // если включен фильтр избранного
            if (showingFavorites)
                FilterSongs();

            OnPropertyChanged(nameof(FilteredSongs));
        }

        /// <summary>
        /// Включает или выключает фильтр отображения только избранных песен.
        /// </summary>
        public void ToggleFavoritesFilter()
        {
            showingFavorites = !showingFavorites;
            FilterSongs();
        }

        // ---------------------------
        // Внутренние методы
        // ---------------------------
        /// <summary>
        /// Применяет фильтры поиска, жанра и избранного к списку песен.
        /// </summary>
        private void FilterSongs()
        {
            FilteredSongs.Clear();

            foreach (var song in Songs)
            {
                bool matchesGenre = SelectedGenre == "Все жанры" || song.Genres.Any(g => g.GenreName == SelectedGenre);

                bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                                     song.Title.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     song.Composer.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     song.Genres.Any(g => g.GenreName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);

                bool matchesFavorites = !showingFavorites || song.IsFavorite;

                if (matchesGenre && matchesSearch && matchesFavorites)
                {
                    FilteredSongs.Add(song);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
