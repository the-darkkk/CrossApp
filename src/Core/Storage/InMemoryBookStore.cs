using System.Collections.Concurrent;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Реалізація сховища книг у пам'яті: ConcurrentDictionary (потокобезпечне для Singleton DI).
/// </summary>
public sealed class InMemoryBookStore : ICatalogStore
{
    private readonly ConcurrentDictionary<string, Book> _items;

    public InMemoryBookStore(IEnumerable<Book>? seed = null)
    {
        _items = new ConcurrentDictionary<string, Book>(
            (seed ?? []).ToDictionary(b => b.Id, StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Повертає список усіх книг у сховищі.
    /// </summary>
    public IReadOnlyList<Book> List() => _items.Values.ToList();

    /// <summary>
    /// Повертає книгу за id або null, якщо ключа немає.
    /// </summary>
    public Book? GetById(string id) => _items.GetValueOrDefault(id);

    /// <summary>
    /// Додає книгу до словника з перевіркою унікальності id.
    /// </summary>
    public void Add(Book item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.TryAdd(item.Id, item))
            throw new DomainException($"Запис з id={item.Id} уже існує.");
    }

    /// <summary>
    /// Оновлює книгу у словнику.
    /// </summary>
    public void Update(Book item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items[item.Id] = item;
    }

    /// <summary>
    /// Видаляє книгу зі словника за id.
    /// </summary>
    public bool Remove(string id) => _items.TryRemove(id, out _);

    /// <summary>
    /// Пошук з предикатом Func&lt;Book, bool&gt;.
    /// </summary>
    public IReadOnlyList<Book> Search(Func<Book, bool> predicate) =>
        _items.Values.Where(predicate).ToList();
}
