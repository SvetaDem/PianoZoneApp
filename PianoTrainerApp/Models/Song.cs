using PianoTrainerApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PianoTrainerApp.Models
{
    public class Song
    {
        public string Title { get; set; }
        public string Composer { get; set; }
        public List<Genre> Genres { get; set; } = new List<Genre>();

        public string ImagePath { get; set; }  // путь к обложке

        public string MidiPath { get; set; } // путь к файлу
        public bool IsFavorite { get; set; } = false;

        public string MetaText
        {
            get
            {
                var genresText = string.Join(", ", Genres.Select(g => g.ToDisplayString()));
                return $"{Composer} • {genresText}";
            }
        }
    }
}
