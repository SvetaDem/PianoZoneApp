using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    /// <summary>
    /// Контекст базы данных для приложения PianoTrainerApp.
    /// Обеспечивает доступ к сущностям Users, Songs, Genres и SongsUsers
    /// и управляет конфигурацией модели Entity Framework.
    /// </summary>
    public class ReMinorContext : DbContext
    {
        /// <summary>
        /// Создает новый экземпляр контекста с подключением к базе данных,
        /// имя подключения берется из App.config (ключ "ReMinorDb").
        /// </summary>
        public ReMinorContext()
            : base("name=ReMinorDb")  // имя из App.config
        { }
        public DbSet<User> Users { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<SongUser> SongsUsers { get; set; }

        /// <summary>
        /// Настраивает модель базы данных перед созданием схемы.
        /// </summary>
        /// <param name="modelBuilder">Конструктор модели для конфигурации сущностей.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("re_minor");

            modelBuilder.Entity<Song>()
                .HasMany(s => s.Genres)
                .WithMany(g => g.Songs)
                .Map(m =>
                {
                    m.ToTable("songs_genres");
                    m.MapLeftKey("id_song");
                    m.MapRightKey("id_genre");
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}
