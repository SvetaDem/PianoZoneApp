using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq; // для Cast и Select
using System.Data.Entity;  // для новой версии Include с лямбдой

using PianoTrainerApp.Models;
using System.Windows;

namespace PianoTrainerApp.ViewModels
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
        public ObservableCollection<Song> FilteredSongs { get; set; } = new ObservableCollection<Song>();

        // Список всех жанров + специальный "ничего не выбрано"
        public ObservableCollection<string> Genres { get; set; } = new ObservableCollection<string>();
        public bool IsUserAuthorized => CurrentUserId != 0;
        public int FavoritesCount =>
    Songs.Count(s => s.IsFavorite);

        public event Action FavoritesChanged;

        public int CurrentUserId { get; set; }

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

        private Song selectedSong;
        public Song SelectedSong
        {
            get => selectedSong;
            set { selectedSong = value; OnPropertyChanged(); }
        }

        public LibraryViewModel()
        {
            CurrentUserId = Properties.Settings.Default.CurrentUserId;
            LoadSongs();

        }

        // Загрузка песен
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
                            song.IsFavorite = song.SongUsers?
                                .Any(su => su.UserId == CurrentUserId && su.IsFavorite) ?? false;
                        }
                    }
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
                MessageBox.Show("Неизвестная ошибка: " + ex.InnerException.Message);
            }

            FilteredSongs = new ObservableCollection<Song>(Songs);
            SelectedGenre = "Все жанры";

            OnPropertyChanged(nameof(FilteredSongs));
            OnPropertyChanged(nameof(Genres));
        }

        // Сброс любимых песен (вызывается при logout)
        public void ResetFavorites()
        {
            CurrentUserId = 0;
            Properties.Settings.Default.CurrentUserId = 0;
            Properties.Settings.Default.Save();

            foreach (var song in Songs)
                song.IsFavorite = false;

            FilteredSongs = new ObservableCollection<Song>(Songs);
            SelectedGenre = "Все жанры";

            OnPropertyChanged(nameof(FilteredSongs));
            OnPropertyChanged(nameof(Genres));
        }

        // Обновление значения CurrentUser
        public void SetCurrentUser(int userId)
        {
            CurrentUserId = userId;
            LoadSongs();
        }

        // Для чтения/записи в таблицу понравившихся песен
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

            // вызов события
            FavoritesChanged?.Invoke();

            // если включен фильтр избранного
            if (showingFavorites)
                FilterSongs();

            OnPropertyChanged(nameof(FilteredSongs));

            
        }

        // Для отображения списка понравившихся песен
        private bool showingFavorites = false;

        public void ToggleFavoritesFilter()
        {
            showingFavorites = !showingFavorites;
            FilterSongs();
        }

        // Фильтрация списка песен
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
