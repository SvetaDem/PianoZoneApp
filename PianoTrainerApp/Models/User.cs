using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PianoTrainerApp.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id_user")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("username")]
        public string Username { get; set; }

        [Required, MaxLength(100)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("last_login_date")]
        public DateTime? LastLoginDate { get; set; }  // дата последнего захода

        [Column("current_streak")]
        public int CurrentStreak { get; set; }  // текущий стрик

        public virtual ICollection<SongUser> FavoriteSongs { get; set; }


    }
}