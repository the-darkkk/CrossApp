using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Аліас сховища каталогу в пам'яті для сумісності з шаблоном завдань.
/// </summary>
public sealed class InMemoryCatalogStore(IEnumerable<Book>? seed = null) : ICatalogStore
{
    private readonly InMemoryBookStore _inner = new(seed);

    public IReadOnlyList<Book> List() => _inner.List();
    public Book? GetById(string id) => _inner.GetById(id);
    public void Add(Book item) => _inner.Add(item);
    public void Update(Book item) => _inner.Update(item);
    public bool Remove(string id) => _inner.Remove(id);
    public IReadOnlyList<Book> Search(Func<Book, bool> predicate) => _inner.Search(predicate);
}

