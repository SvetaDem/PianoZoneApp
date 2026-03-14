using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    [Table("genres")]
    public class Genre
    {
        [Key]
        [Column("id_genre")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("genre_name")]
        public string GenreName { get; set; }

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}
