using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    public class ReMinorContext : DbContext
    {
        public ReMinorContext()
            : base("name=ReMinorDb")  // имя из App.config
        { }
        public DbSet<User> Users { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<SongUser> SongsUsers { get; set; }

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
