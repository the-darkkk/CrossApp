using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

/// <summary>
/// Сервіс бібліотечних операцій: бізнес-логіка додавання книг, видачі та повернення примірників.
/// Підтримує доменні події Changed та опційний INotificationSink.
/// </summary>
public sealed class LendingService(IBookStore store, INotificationSink? sink = null)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly INotificationSink? _sink = sink;

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    private void OnChanged(
        Book p,
        CatalogChangeKind kind,
        int delta,
        string? copyId = null,
        string? readerId = null,
        DateTimeOffset? dueDate = null)
    {
        int availableCopies = p.Copies.Count(c => !c.IsIssued);
        var args = new CatalogChangedEventArgs(
            p.Id,
            kind,
            delta,
            availableCopies,
            copyId,
            readerId,
            dueDate);

        Changed?.Invoke(this, args);
        _sink?.Emit($"{kind}: {p.Id} ({p.Title})");
    }

    /// <summary>
    /// Додає нову книгу до каталогу бібліотеки.
    /// </summary>
    public Book AddBook(string isbn, string title, int year, string? author = null, int initialCopies = 1)
    {
        var book = Book.Create(Guid.NewGuid().ToString("N")[..8], isbn, title, year, author);
        for (int i = 1; i <= initialCopies; i++)
        {
            book.AddCopy($"BC-{book.Id}-{i:D2}");
        }
        _store.Add(book);
        OnChanged(book, CatalogChangeKind.BookAdded, delta: initialCopies > 0 ? initialCopies : 1);
        return book;
    }

    /// <summary>
    /// Додає книгу з відомим id.
    /// </summary>
    public Book AddBook(string id, string isbn, string title, int year, string? author = null, int initialCopies = 0)
    {
        var book = Book.Create(id, isbn, title, year, author);
        for (int i = 1; i <= initialCopies; i++)
        {
            book.AddCopy($"BC-{book.Id}-{i:D2}");
        }
        _store.Add(book);
        OnChanged(book, CatalogChangeKind.BookAdded, delta: initialCopies > 0 ? initialCopies : 1);
        return book;
    }

    /// <summary>
    /// Постачання нових примірників книги до фонду бібліотеки.
    /// </summary>
    public void Receive(string bookId, int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Кількість примірників має бути більшою за 0.");

        var book = _store.GetById(bookId)
            ?? throw new DomainException($"Немає книги з id={bookId}.");

        for (int i = 1; i <= count; i++)
        {
            book.AddCopy($"BC-{book.Id}-{Guid.NewGuid().ToString("N")[..4]}");
        }
        _store.Update(book);
        OnChanged(book, CatalogChangeKind.Received, delta: count);
    }

    /// <summary>
    /// Реєструє новий фізичний примірник книги та оновлює сховище.
    /// </summary>
    public BookCopy RegisterCopy(string bookId, string copyId)
    {
        var book = _store.GetById(bookId)
            ?? throw new DomainException($"Немає книги з id={bookId}.");
        var copy = book.AddCopy(copyId);
        _store.Update(book);
        OnChanged(book, CatalogChangeKind.BookAdded, delta: 1, copyId: copy.Id);
        return copy;
    }

    /// <summary>
    /// Видача примірника книги читачу.
    /// Подія піднімається лише після успішної зміни стану.
    /// </summary>
    public Loan IssueCopy(string bookId, string copyId, string readerId, DateTimeOffset? dueDate = null)
    {
        var book = _store.GetById(bookId)
            ?? throw new DomainException($"Немає книги з id={bookId}.");

        var copy = book.Copies.FirstOrDefault(c => c.Id.Equals(copyId, StringComparison.OrdinalIgnoreCase))
            ?? throw new DomainException($"Примірник {copyId} не знайдено у книзі {bookId}.");

        var loan = Loan.Open(
            Guid.NewGuid().ToString("N")[..8],
            copy,
            readerId,
            DateTime.Now);

        _store.Update(book);
        DateTimeOffset due = dueDate ?? DateTimeOffset.Now.AddDays(14);
        OnChanged(book, CatalogChangeKind.CopyIssued, delta: -1, copyId: copy.Id, readerId: readerId, dueDate: due);
        return loan;
    }

    /// <summary>
    /// Повернення примірника книги.
    /// </summary>
    public void ReturnCopy(string bookId, string copyId, Loan loan)
    {
        var book = _store.GetById(bookId)
            ?? throw new DomainException($"Немає книги з id={bookId}.");

        var copy = book.Copies.FirstOrDefault(c => c.Id.Equals(copyId, StringComparison.OrdinalIgnoreCase))
            ?? throw new DomainException($"Примірник {copyId} не знайдено у книзі {bookId}.");

        loan.Close(DateTime.Now, copy);
        _store.Update(book);
        OnChanged(book, CatalogChangeKind.CopyReturned, delta: 1, copyId: copy.Id, readerId: loan.ReaderId);
    }

    /// <summary>
    /// Повертає список усіх книг у каталозі.
    /// </summary>
    public IReadOnlyList<Book> All() => _store.List();

    /// <summary>
    /// Знаходить книгу за ідентифікатором.
    /// </summary>
    public Book? Find(string id) => _store.GetById(id);

    /// <summary>
    /// Видаляє книгу зі сховища.
    /// </summary>
    public bool Remove(string id) => _store.Remove(id);

    /// <summary>
    /// Пошук книг за предикатом (Func-делегат замість події для отримання результату).
    /// </summary>
    public IReadOnlyList<Book> FindAll(Func<Book, bool> predicate) =>
        _store.List().Where(predicate).ToList();

    public IReadOnlyList<Book> Search(Func<Book, bool> predicate) => FindAll(predicate);

    /// <summary>
    /// Асинхронний імпорт каталогу книг із CancellationToken та необов'язковим звітом про прогрес (тиждень 8).
    /// </summary>
    public async Task<int> ImportAsync(
        ICatalogImporter importer,
        string path,
        CancellationToken ct = default)
    {
        return await ImportAsync(importer, path, progress: null, ct);
    }

    /// <summary>
    /// Асинхронний імпорт із прогресом IProgress (Додаткове завдання 3).
    /// </summary>
    public async Task<int> ImportAsync(
        ICatalogImporter importer,
        string path,
        IProgress<int>? progress,
        CancellationToken ct = default)
    {
        var items = await importer.LoadAsync(path, ct);
        var added = 0;
        for (var i = 0; i < items.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var item = items[i];
            if (_store.GetById(item.Id) is null)
            {
                _store.Add(item);
                OnChanged(item, CatalogChangeKind.BookAdded, item.Quantity);
                added++;
            }
            progress?.Report((int)((i + 1) * 100.0 / items.Count));
        }
        return added;
    }

    /// <summary>
    /// Потоковий імпорт через IAsyncEnumerable з await foreach (Додаткове завдання 1).
    /// </summary>
    public async Task<int> ImportStreamAsync(
        ICatalogImporter importer,
        string path,
        IProgress<int>? progress = null,
        CancellationToken ct = default)
    {
        var added = 0;
        await foreach (var item in importer.ReadAsync(path, ct).WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            if (_store.GetById(item.Id) is null)
            {
                _store.Add(item);
                OnChanged(item, CatalogChangeKind.BookAdded, item.Quantity);
                added++;
            }
        }
        return added;
    }
}
