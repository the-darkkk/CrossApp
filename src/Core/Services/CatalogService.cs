using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

/// <summary>
/// Аліас сервісу каталогу для відповідності шаблону методички.
/// </summary>
public sealed class CatalogService(IBookStore store, INotificationSink? sink = null)
{
    private readonly LendingService _inner = new(store, sink);

    public event EventHandler<CatalogChangedEventArgs>? Changed
    {
        add => _inner.Changed += value;
        remove => _inner.Changed -= value;
    }

    public Book Add(string isbn, string title, int year, string? author = null, int initialCopies = 1) =>
        _inner.AddBook(isbn, title, year, author, initialCopies);

    public void Receive(string id, int count) =>
        _inner.Receive(id, count);

    public BookCopy RegisterCopy(string bookId, string copyId) =>
        _inner.RegisterCopy(bookId, copyId);

    public Loan IssueCopy(string bookId, string copyId, string readerId, DateTimeOffset? dueDate = null) =>
        _inner.IssueCopy(bookId, copyId, readerId, dueDate);

    public void ReturnCopy(string bookId, string copyId, Loan loan) =>
        _inner.ReturnCopy(bookId, copyId, loan);

    public IReadOnlyList<Book> All() => _inner.All();

    public Book? Find(string id) => _inner.Find(id);

    public IReadOnlyList<Book> FindAll(Func<Book, bool> predicate) => _inner.FindAll(predicate);
}
