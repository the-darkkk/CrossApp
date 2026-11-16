using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BookRow> Books => Set<BookRow>();
    public DbSet<CopyRow> Copies => Set<CopyRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var book = modelBuilder.Entity<BookRow>();

        book.ToTable("Books");
        book.HasKey(b => b.Id);
        book.Property(b => b.Id).HasMaxLength(32);
        book.Property(b => b.Isbn).IsRequired().HasMaxLength(32);
        book.Property(b => b.Title).IsRequired().HasMaxLength(200);
        book.Property(b => b.Author).HasMaxLength(100);
        book.Property(b => b.Unit).IsRequired().HasMaxLength(20);
        book.Property(b => b.Genre).HasMaxLength(50);

        book.HasIndex(b => b.Isbn).IsUnique();

        book.HasMany(b => b.Copies)
            .WithOne(c => c.Book)
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        var copy = modelBuilder.Entity<CopyRow>();

        copy.ToTable("Copies");
        copy.HasKey(c => c.Id);
        copy.Property(c => c.Id).HasMaxLength(64);
        copy.Property(c => c.BookId).IsRequired().HasMaxLength(32);
        copy.Property(c => c.Isbn).IsRequired().HasMaxLength(32);
        copy.Property(c => c.IsIssued).IsRequired();
    }
}
