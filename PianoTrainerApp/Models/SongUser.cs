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

        [Column("accuracy")]
        public double Accuracy { get; set; }

        [Column("best_accuracy")]
        public double BestAccuracy { get; set; }

        [ForeignKey("SongId")]  // Имя свойства в классе, которое играет роль внешнего ключа, а не имя колонки в базе
        public virtual Song Song { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [NotMapped]
        public int Stars
        {
            get
            {
                if (BestAccuracy < 0.5) return 0;
                if (BestAccuracy < 0.7) return 1;
                if (BestAccuracy < 0.9) return 2;
                return 3;
            }
        }
    }
}