using PianoTrainerApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PianoTrainerApp.Models
{
    [Table("songs")]
    public class Song
    {
        [Key]
        [Column("id_song")]
        public int Id { get; set; }
        [Required, MaxLength(200)]
        [Column("title")]
        public string Title { get; set; }
        [MaxLength(100)]
        [Column("composer")]
        public string Composer { get; set; }
        [Column("image_path")]

        public string ImagePath { get; set; }  // путь к обложке
        [Column("midi_path")]
        public string MidiPath { get; set; } // путь к файлу
        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
        public virtual ICollection<SongUser> SongUsers { get; set; } = new List<SongUser>();

        [NotMapped]
        public bool IsFavorite { get; set; }
        public string MetaText
        {
            get
            {
                var genresText = string.Join(", ", Genres.Select(g => g.GenreName));
                return $"{Composer} • {genresText}";
            }
        }
    }
}
