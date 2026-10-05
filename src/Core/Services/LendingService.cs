using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

/// <summary>
/// Сервіс бібліотечних операцій: бізнес-логіка додавання книг, видачі та повернення примірників.
/// Залежить лише від абстракції IBookStore, а не від конкретного класу.
/// Ін'єкція залежності через конструктор (ручний DI без контейнера).
/// </summary>
public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    /// <summary>
    /// Додає нову книгу до каталогу бібліотеки.
    /// Інваріанти перевіряє Book.Create, сховище перевіряє унікальність id.
    /// </summary>
    public Book AddBook(string isbn, string title, int year, string? author = null)
    {
        var book = Book.Create(Guid.NewGuid().ToString("N")[..8], isbn, title, year, author);
        _store.Add(book);
        return book;
    }

    /// <summary>
    /// Додає книгу з відомим id (наприклад, при імпорті з зовнішнього джерела).
    /// </summary>
    public Book AddBook(string id, string isbn, string title, int year, string? author = null)
    {
        var book = Book.Create(id, isbn, title, year, author);
        _store.Add(book);
        return book;
    }

    /// <summary>
    /// Реєструє новий фізичний примірник книги та оновлює сховище.
    /// Метод сутності Book.AddCopy з тижня 4 перевіряє унікальність примірника.
    /// </summary>
    public BookCopy RegisterCopy(string bookId, string copyId)
    {
        var book = _store.GetById(bookId)
            ?? throw new InvalidOperationException($"Немає книги з id={bookId}.");
        var copy = book.AddCopy(copyId);
        _store.Update(book);
        return copy;
    }

    /// <summary>
    /// Видача примірника книги читачу: створює Loan та змінює стан примірника.
    /// </summary>
    public Loan IssueCopy(string bookId, string copyId, string readerId)
    {
        var book = _store.GetById(bookId)
            ?? throw new InvalidOperationException($"Немає книги з id={bookId}.");

        var copy = book.Copies.FirstOrDefault(c => c.Id.Equals(copyId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Примірник {copyId} не знайдено у книзі {bookId}.");

        var loan = Loan.Open(
            Guid.NewGuid().ToString("N")[..8],
            copy,
            readerId,
            DateTime.Now);

        _store.Update(book);
        return loan;
    }

    /// <summary>
    /// Повернення примірника книги: закриває видачу та повертає примірник до фонду.
    /// </summary>
    public void ReturnCopy(string bookId, string copyId, Loan loan)
    {
        var book = _store.GetById(bookId)
            ?? throw new InvalidOperationException($"Немає книги з id={bookId}.");

        var copy = book.Copies.FirstOrDefault(c => c.Id.Equals(copyId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Примірник {copyId} не знайдено у книзі {bookId}.");

        loan.Close(DateTime.Now, copy);
        _store.Update(book);
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
    /// Додаткове завдання 2: Пошук книг за предикатом (підготовка до тижнів 6-7).
    /// </summary>
    public IReadOnlyList<Book> Search(Func<Book, bool> predicate) => _store.Search(predicate);
}
