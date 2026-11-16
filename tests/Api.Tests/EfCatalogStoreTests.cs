using Core;
using Core.Domain;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests;

public class EfCatalogStoreTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly EfCatalogStore _store;

    public EfCatalogStoreTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _store = new EfCatalogStore(_context);
    }

    [Fact]
    public void Add_And_GetById_ReturnsDomainBookWithCopies()
    {
        var book = Book.Create("B-TEST-01", "978-1-11-111111-1", "Test Driven Development", 2002, "Kent Beck");
        book.AddCopy("CP-01");
        book.AddCopy("CP-02");

        _store.Add(book);

        var retrieved = _store.GetById("B-TEST-01");
        Assert.NotNull(retrieved);
        Assert.Equal("Test Driven Development", retrieved.Title);
        Assert.Equal("978-1-11-111111-1", retrieved.Isbn);
        Assert.Equal(2, retrieved.Copies.Count);
    }

    [Fact]
    public void Add_DuplicateIsbn_ThrowsDomainException()
    {
        var b1 = Book.Create("B-1", "978-2-22-222222-2", "Book 1", 2020);
        _store.Add(b1);

        var b2 = Book.Create("B-2", "978-2-22-222222-2", "Book 2", 2021);
        Assert.Throws<DomainException>(() => _store.Add(b2));
    }

    [Fact]
    public void Update_ModifiesPropertiesAndSyncsCopies()
    {
        var book = Book.Create("B-UP", "978-3-33-333333-3", "Initial Title", 2020, "Author");
        book.AddCopy("CP-U1");
        _store.Add(book);

        var updatedBook = Book.Create("B-UP", "978-3-33-333333-3", "New Title", 2021, "Author");
        updatedBook.AddCopy("CP-U1");
        updatedBook.AddCopy("CP-U2");
        _store.Update(updatedBook);

        var retrieved = _store.GetById("B-UP");
        Assert.NotNull(retrieved);
        Assert.Equal("New Title", retrieved.Title);
        Assert.Equal(2, retrieved.Copies.Count);
    }

    [Fact]
    public void Remove_DeletesBookAndCascadesCopies()
    {
        var book = Book.Create("B-DEL", "978-4-44-444444-4", "To Delete", 2020);
        book.AddCopy("CP-D1");
        _store.Add(book);

        var removed = _store.Remove("B-DEL");
        Assert.True(removed);
        Assert.Null(_store.GetById("B-DEL"));
        Assert.Empty(_context.Copies.Where(c => c.BookId == "B-DEL"));
    }

    [Fact]
    public async Task AsyncOperations_WorkAsExpected()
    {
        var book = Book.Create("B-ASYNC", "978-5-55-555555-5", "Async Book", 2023);
        book.AddCopy("CP-A1");

        await _store.AddAsync(book);

        var retrieved = await _store.GetByIdAsync("B-ASYNC");
        Assert.NotNull(retrieved);
        Assert.Equal("Async Book", retrieved.Title);

        var list = await _store.ListAsync();
        Assert.Contains(list, b => b.Id == "B-ASYNC");

        var removed = await _store.RemoveAsync("B-ASYNC");
        Assert.True(removed);
        Assert.Null(await _store.GetByIdAsync("B-ASYNC"));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
