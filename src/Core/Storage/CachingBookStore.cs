using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Додаткове завдання 1: Декоратор, який приймає інший IBookStore у конструкторі.
/// Той самий контракт — нова поведінка: кешує результат List() і GetById()
/// доти, доки не відбудеться Add/Update/Remove (інвалідація кешу).
/// </summary>
public sealed class CachingBookStore(IBookStore inner) : IBookStore
{
    private readonly IBookStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private IReadOnlyList<Book>? _listCache;

    /// <summary>
    /// Повертає кешований список або делегує до внутрішнього сховища.
    /// </summary>
    public IReadOnlyList<Book> List()
    {
        _listCache ??= _inner.List();
        return _listCache;
    }

    /// <summary>
    /// Делегує пошук за id до внутрішнього сховища.
    /// </summary>
    public Book? GetById(string id) => _inner.GetById(id);

    /// <summary>
    /// Додає книгу через внутрішнє сховище та інвалідує кеш.
    /// </summary>
    public void Add(Book item)
    {
        _inner.Add(item);
        _listCache = null;
    }

    /// <summary>
    /// Оновлює книгу через внутрішнє сховище та інвалідує кеш.
    /// </summary>
    public void Update(Book item)
    {
        _inner.Update(item);
        _listCache = null;
    }

    /// <summary>
    /// Видаляє книгу через внутрішнє сховище та інвалідує кеш.
    /// </summary>
    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed) _listCache = null;
        return removed;
    }

    /// <summary>
    /// Додаткове завдання 2: Пошук з предикатом.
    /// </summary>
    public IReadOnlyList<Book> Search(Func<Book, bool> predicate) =>
        _inner.Search(predicate);
}
