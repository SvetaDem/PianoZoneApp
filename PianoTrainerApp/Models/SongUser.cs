using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PianoTrainerApp.Models
{
    [Table("songs_users")]
    public class SongUser
    {
        [Key, Column("id_song", Order = 0)]
        public int SongId { get; set; }

        [Key, Column("id_user", Order = 1)]
        public int UserId { get; set; }
        [Column("is_favorite")]
        public bool IsFavorite { get; set; }

        [ForeignKey("SongId")]  // Имя свойства в классе, которое играет роль внешнего ключа, а не имя колонки в базе
        public virtual Song Song { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}