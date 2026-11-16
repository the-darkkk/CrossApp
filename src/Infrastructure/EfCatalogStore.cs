using Core;
using Core.Abstractions;
using Core.Domain;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfCatalogStore(AppDbContext db) : ICatalogStore, IAsyncCatalogStore
{
    private static Book ToDomain(BookRow row)
    {
        var book = Book.Create(row.Id, row.Isbn, row.Title, row.Year, row.Author, row.Unit, row.Genre);
        if (row.Copies is not null)
        {
            foreach (var copyRow in row.Copies)
            {
                var copy = book.AddCopy(copyRow.Id);
                if (copyRow.IsIssued)
                {
                    copy.Issue();
                }
            }
        }
        return book;
    }

    private static BookRow ToRow(Book b)
    {
        var row = new BookRow
        {
            Id = b.Id,
            Isbn = b.Isbn,
            Title = b.Title,
            Year = b.Year,
            Author = b.Author,
            Unit = b.Unit,
            Genre = b.Genre
        };

        foreach (var copy in b.Copies)
        {
            row.Copies.Add(new CopyRow
            {
                Id = copy.Id,
                BookId = b.Id,
                Isbn = copy.Isbn,
                IsIssued = copy.IsIssued
            });
        }

        return row;
    }

    public IReadOnlyList<Book> List() =>
        db.Books.AsNoTracking()
            .Include(b => b.Copies)
            .OrderBy(x => x.Isbn)
            .AsEnumerable()
            .Select(ToDomain)
            .ToList();

    public async Task<IReadOnlyList<Book>> ListAsync(CancellationToken ct = default)
    {
        var rows = await db.Books.AsNoTracking()
            .Include(b => b.Copies)
            .OrderBy(x => x.Isbn)
            .ToListAsync(ct);

        return rows.Select(ToDomain).ToList();
    }

    public Book? GetById(string id) =>
        db.Books.AsNoTracking()
            .Include(b => b.Copies)
            .SingleOrDefault(x => x.Id == id) is { } row
            ? ToDomain(row)
            : null;

    public async Task<Book?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var row = await db.Books.AsNoTracking()
            .Include(b => b.Copies)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        return row is not null ? ToDomain(row) : null;
    }

    public void Add(Book item)
    {
        if (db.Books.Any(b => b.Isbn == item.Isbn))
        {
            throw new DomainException($"Книга з ISBN '{item.Isbn}' вже існує.");
        }

        if (db.Books.Any(b => b.Id == item.Id))
        {
            throw new DomainException($"Книга з ID '{item.Id}' вже існує.");
        }

        db.Books.Add(ToRow(item));

        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            throw new DomainException($"Помилка збереження сутності: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
    }

    public async Task AddAsync(Book item, CancellationToken ct = default)
    {
        if (await db.Books.AnyAsync(b => b.Isbn == item.Isbn, ct))
        {
            throw new DomainException($"Книга з ISBN '{item.Isbn}' вже існує.");
        }

        if (await db.Books.AnyAsync(b => b.Id == item.Id, ct))
        {
            throw new DomainException($"Книга з ID '{item.Id}' вже існує.");
        }

        await db.Books.AddAsync(ToRow(item), ct);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            throw new DomainException($"Помилка збереження сутності: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
    }

    public void Update(Book item)
    {
        var row = db.Books.Include(b => b.Copies).SingleOrDefault(x => x.Id == item.Id);
        if (row is null)
            return;

        if (row.Isbn != item.Isbn && db.Books.Any(b => b.Isbn == item.Isbn && b.Id != item.Id))
        {
            throw new DomainException($"Книга з ISBN '{item.Isbn}' вже існує.");
        }

        row.Isbn = item.Isbn;
        row.Title = item.Title;
        row.Year = item.Year;
        row.Author = item.Author;
        row.Unit = item.Unit;
        row.Genre = item.Genre;

        row.Copies.Clear();
        foreach (var copy in item.Copies)
        {
            row.Copies.Add(new CopyRow
            {
                Id = copy.Id,
                BookId = item.Id,
                Isbn = copy.Isbn,
                IsIssued = copy.IsIssued
            });
        }

        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            throw new DomainException($"Помилка збереження оновлень: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(Book item, CancellationToken ct = default)
    {
        var row = await db.Books.Include(b => b.Copies).SingleOrDefaultAsync(x => x.Id == item.Id, ct);
        if (row is null)
            return;

        if (row.Isbn != item.Isbn && await db.Books.AnyAsync(b => b.Isbn == item.Isbn && b.Id != item.Id, ct))
        {
            throw new DomainException($"Книга з ISBN '{item.Isbn}' вже існує.");
        }

        row.Isbn = item.Isbn;
        row.Title = item.Title;
        row.Year = item.Year;
        row.Author = item.Author;
        row.Unit = item.Unit;
        row.Genre = item.Genre;

        row.Copies.Clear();
        foreach (var copy in item.Copies)
        {
            row.Copies.Add(new CopyRow
            {
                Id = copy.Id,
                BookId = item.Id,
                Isbn = copy.Isbn,
                IsIssued = copy.IsIssued
            });
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            throw new DomainException($"Помилка збереження оновлень: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
    }

    public bool Remove(string id)
    {
        var row = db.Books.SingleOrDefault(x => x.Id == id);
        if (row is null) return false;

        db.Books.Remove(row);
        db.SaveChanges();
        return true;
    }

    public async Task<bool> RemoveAsync(string id, CancellationToken ct = default)
    {
        var row = await db.Books.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (row is null) return false;

        db.Books.Remove(row);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public IReadOnlyList<Book> Search(Func<Book, bool> predicate) =>
        List().Where(predicate).ToList();
}
