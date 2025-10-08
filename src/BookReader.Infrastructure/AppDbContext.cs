using Microsoft.EntityFrameworkCore;
using BookReader.Domain;
using System.IO;

namespace BookReader.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<BookTag> BookTags => Set<BookTag>();
    public DbSet<MainGroup> MainGroups => Set<MainGroup>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BookReader");
        Directory.CreateDirectory(dir);
        var dbPath = Path.Combine(dir, "bookreader.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Book>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired();
            e.HasIndex(x => x.Title);
            e.HasOne(x => x.MainGroup)
             .WithMany(g => g.Books)
             .HasForeignKey(x => x.MainGroupId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Tag>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<BookTag>(e =>
        {
            e.HasKey(x => new { x.BookId, x.TagId });
            e.HasOne(x => x.Book).WithMany(bk => bk.BookTags).HasForeignKey(x => x.BookId);
            e.HasOne(x => x.Tag).WithMany(t => t.BookTags).HasForeignKey(x => x.TagId);
        });

        b.Entity<MainGroup>().HasData(
            new MainGroup { Id = 1, Name = "Beletrie" },
            new MainGroup { Id = 2, Name = "Naučná" },
            new MainGroup { Id = 3, Name = "Dětská" },
            new MainGroup { Id = 4, Name = "Technická" },
            new MainGroup { Id = 5, Name = "Historie" }
        );
    }

    public static AppDbContext Create() => new();
}
