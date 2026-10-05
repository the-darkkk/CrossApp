using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Реалізація сховища книг у пам'яті: Dictionary + початкові дані через конструктор.
/// Після виходу з програми дані зникають.
/// </summary>
public sealed class InMemoryBookStore(IEnumerable<Book>? seed = null) : IBookStore
{
    private readonly Dictionary<string, Book> _items =
        (seed ?? []).ToDictionary(b => b.Id, StringComparer.OrdinalIgnoreCase);

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
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
        _items.Add(item.Id, item);
    }

    /// <summary>
    /// Оновлює книгу у словнику.
    /// </summary>
    public void Update(Book item) => _items[item.Id] = item;

    /// <summary>
    /// Видаляє книгу зі словника за id.
    /// </summary>
    public bool Remove(string id) => _items.Remove(id);

    /// <summary>
    /// Додаткове завдання 2: Пошук з предикатом Func&lt;Book, bool&gt;.
    /// </summary>
    public IReadOnlyList<Book> Search(Func<Book, bool> predicate) =>
        _items.Values.Where(predicate).ToList();
}
