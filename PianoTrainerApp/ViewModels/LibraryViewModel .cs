using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq; // для Cast и Select

using PianoTrainerApp.Models;

namespace PianoTrainerApp.ViewModels
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Song> Songs { get; set; }
        public ObservableCollection<Song> FilteredSongs { get; set; }

        // Список всех жанров + специальный "ничего не выбрано"
        public ObservableCollection<string> Genres { get; set; }

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
            // Инициализация жанров
            Genres = new ObservableCollection<string> { "Choose the Genre" };
            var enumValues = Enum.GetValues(typeof(Genre)).Cast<Genre>();
            var genreNames = enumValues.Select(g =>
            {
                if (g == Genre.RB) return "R&B";
                if (g == Genre.HipHop) return "Hip hop";
                return g.ToString();
            }).OrderBy(x => x);
            foreach (var g in genreNames) Genres.Add(g);

            // Песни
            Songs = new ObservableCollection<Song>
            {
                new Song {
                    Title = "Twinkle Little Star",
                    Composer = "Unknown",
                    MidiPath = "Assets/twinkle-twinkle-little-star.mid",
                    ImagePath = "/Assets/covers/twinkle.jpg",
                    Genres = new List<Genre>{ Genre.Classical, Genre.Folk }
                },
                new Song {
                    Title = "Für Elise",
                    Composer = "Beethoven",
                    MidiPath = "Assets/Fur Elise.mid",
                    ImagePath = "/Assets/covers/bethoven.jpg",
                    Genres = new List<Genre>{ Genre.Classical, Genre.RB }
                }
            };

            FilteredSongs = new ObservableCollection<Song>(Songs);

            // По умолчанию
            SelectedGenre = "Choose the Genre";
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
                bool matchesGenre = SelectedGenre == "Choose the Genre" || song.Genres.Any(g =>
                    (g == Genre.RB && SelectedGenre == "R&B") ||
                    (g == Genre.HipHop && SelectedGenre == "Hip hop") ||
                    g.ToString() == SelectedGenre.Replace(" ", "")
                );

                bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                                     song.Title.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     song.Composer.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     song.Genres.Any(g => (g == Genre.RB && "R&B".IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                                          (g == Genre.HipHop && "Hip hop".IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                                          g.ToString().IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0
                                                     );

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
