using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.ViewModels
{
    public class LibraryViewModel
    {
        public ObservableCollection<Song> Songs { get; set; }
        public ObservableCollection<DifficultyLevel> DifficultyLevels { get; set; }

        private DifficultyLevel selectedDifficulty;
        public DifficultyLevel SelectedDifficulty
        {
            get => selectedDifficulty;
            set
            {
                selectedDifficulty = value;
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

        public ObservableCollection<Song> FilteredSongs { get; set; }

        public LibraryViewModel()
        {
            DifficultyLevels = new ObservableCollection<DifficultyLevel>
            {
                DifficultyLevel.Beginner,
                DifficultyLevel.Intermediate,
                DifficultyLevel.Advanced
            };

            Songs = new ObservableCollection<Song>
            {
                new Song { Title="Twinkle Little Star", Difficulty=DifficultyLevel.Beginner, MidiPath="Assets/twinkle-twinkle-little-star.mid" },
                new Song { Title="Für Elise", Difficulty=DifficultyLevel.Intermediate, MidiPath="Assets/Fur Elise.mid" },
                new Song { Title="Moonlight Sonata", Difficulty=DifficultyLevel.Advanced, MidiPath="Assets/moonlight.mid" }
            };

            FilteredSongs = new ObservableCollection<Song>(Songs);
        }

        private void FilterSongs()
        {
            FilteredSongs.Clear();
            foreach (var s in Songs)
                if (s.Difficulty == SelectedDifficulty)
                    FilteredSongs.Add(s);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
