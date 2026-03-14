using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq; // для Cast и Select
using System.Data.Entity;  // для новой версии Include с лямбдой

using PianoTrainerApp.Models;

namespace PianoTrainerApp.ViewModels
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
        public ObservableCollection<Song> FilteredSongs { get; set; } = new ObservableCollection<Song>();

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

        public LibraryViewModel(int? currentUserId=null)
        {
            // Инициализация жанров
            //Genres = new ObservableCollection<string> { "Выберите жанр" };
            //var enumValues = Enum.GetValues(typeof(Genre)).Cast<Genre>();
            //var genreNames = enumValues.Select(g =>
            //  {
            //if (g == Genre.RB) return "R&B";
            // if (g == Genre.HipHop) return "Hip hop";
            //       return g.ToString();
            // }).OrderBy(x => x);
            // foreach (var g in genreNames) Genres.Add(g);

            // Загрузка песен и жанров из БД
            using (var db = new ReMinorContext())
            {
                // Загружаем все песни с жанрами
                var songsFromDb = db.Songs.Include(s => s.Genres).Include(s => s.SongUsers).ToList();
                Songs = new ObservableCollection<Song>(songsFromDb);

                // Формируем список жанров
                var allGenres = db.Genres.Select(g => g.GenreName).OrderBy(x => x).ToList();
                Genres.Add("Все жанры"); // специальный пункт
                foreach (var g in allGenres) Genres.Add(g);

                // Если есть текущий пользователь — отмечаем любимые песни
                if (currentUserId.HasValue)
                {
                    foreach (var song in Songs)
                    {
                        song.IsFavorite = song.SongUsers?.Any(su => su.UserId == currentUserId && su.IsFavorite) ?? false;
                    }
                }
            }

            FilteredSongs = new ObservableCollection<Song>(Songs);

            // По умолчанию
            SelectedGenre = "Все жанры";
        
        // Песни
        //Songs = new ObservableCollection<Song>
        //{
        //    new Song {
        //        Title = "Test 1 (HZ)",
        //        Composer = "Wolfgang Amadeus Mozart",
        //        MidiPath = "Assets/hz.mid",
        //        ImagePath = "/Assets/covers/twinkle.jpg",
        //        Genres = new List<Genre>{ Genre.Classical, Genre.Folk }
        //    },
        //    new Song {
        //        Title = "Test 2 (Titanic)",
        //        Composer = "Wolfgang Amadeus Mozart",
        //        MidiPath = "Assets/titicaca.mid",
        //        ImagePath = "/Assets/covers/twinkle.jpg",
        //        Genres = new List<Genre>{ Genre.Classical, Genre.Folk }
        //    },
        //    new Song {
        //        Title = "Twinkle Little Star",
        //        Composer = "Wolfgang Amadeus Mozart",
        //        MidiPath = "Assets/twinkle-twinkle-little-star.mid",
        //        ImagePath = "/Assets/covers/twinkle.jpg",
        //        Genres = new List<Genre>{ Genre.Classical, Genre.Folk }
        //    },
        //    new Song {
        //        Title = "Für Elise",
        //        Composer = "Beethoven",
        //        MidiPath = "Assets/Fur Elise.mid",
        //        ImagePath = "/Assets/covers/beethoven.jpg",
        //        Genres = new List<Genre>{ Genre.Classical, Genre.RB }
        //    },
        //    new Song {
        //        Title = "Moonlight Sonata (Piano Sonata No. 14)",
        //        Composer = "Beethoven",
        //        MidiPath = "Assets/moonlight.mid",
        //        ImagePath = "/Assets/covers/beethoven.jpg",
        //        Genres = new List<Genre>{ Genre.Classical }
        //    },
        //    new Song {
        //        Title = "Bear lullaby - Umka",
        //        Composer = "Yuri Yakovlev",
        //        MidiPath = "Assets/Umka, the lullaby.mid",
        //        ImagePath = "/Assets/covers/umka.jpg",
        //        Genres = new List<Genre>{ Genre.Classical, Genre.Soundtrack }
        //    },
        //    new Song {
        //        Title = "My heart will go on",
        //        Composer = "James Horner",
        //        MidiPath = "Assets/Celine Dion - My Heart Will Go On - Titanic Theme - EASY.mid",
        //        ImagePath = "/Assets/covers/titanic.jpg",
        //        Genres = new List<Genre>{ Genre.Pop, Genre.Soundtrack }
        //    },
        //    new Song {
        //        Title = "In The Name Of Love",
        //        Composer = "Martin Garrix",
        //        MidiPath = "Assets/Martin Garrix - In The Name Of Love.mid",
        //        ImagePath = "/Assets/covers/In_the_name_of_love.jpg",
        //        Genres = new List<Genre>{ Genre.Pop, Genre.Electronic}
        //    },
        //    new Song {
        //        Title = "Pirates Of The Caribbean",
        //        Composer = "Hans Zimmer",
        //        MidiPath = "Assets/Pirates Of The Caribbean - He's a Pirate - EASY.mid",
        //        ImagePath = "/Assets/covers/pirates_caribbean.jpg",
        //        Genres = new List<Genre>{ Genre.Soundtrack }
        //    },
        //    new Song {
        //        Title = "Requiem for a Dream - Main Theme",
        //        Composer = "Clint Mansell",
        //        MidiPath = "Assets/Requiem for a Dream - Main Theme - EASY.mid",
        //        ImagePath = "/Assets/covers/requiem_for_a_dream.jpg",
        //        Genres = new List<Genre>{ Genre.Soundtrack, Genre.Electronic }
        //    },
        //    new Song {
        //        Title = "The Sound Of Silence",
        //        Composer = "Paul Simon",
        //        MidiPath = "Assets/The Sound Of Silence - EASY.mid",
        //        ImagePath = "/Assets/covers/sounds_of_Silence.jpg",
        //        Genres = new List<Genre>{ Genre.Folk, Genre.Rock }
        //    },
        //    new Song {
        //        Title = "River Flows in you",
        //        Composer = "Yurima",
        //        MidiPath = "Assets/Yurima - River Flows in you - EASY.mid",
        //        ImagePath = "/Assets/covers/river_flows_in_you.jpg",
        //        Genres = new List<Genre>{ Genre.Classical }
        //    }
        //};

        
        }

        private bool showingFavorites = false;

        public void ToggleFavoritesFilter()
        {
            showingFavorites = !showingFavorites;
            FilterSongs();
        }

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
