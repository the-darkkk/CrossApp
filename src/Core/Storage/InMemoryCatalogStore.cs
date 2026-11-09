using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Аліас сховища каталогу в пам'яті для сумісності з шаблоном завдань та DI.
/// </summary>
public sealed class InMemoryCatalogStore : ICatalogStore
{
    private readonly InMemoryBookStore _inner;

    public InMemoryCatalogStore() : this(SampleData.Books()) { }

    public InMemoryCatalogStore(IEnumerable<Book>? seed)
    {
        _inner = new InMemoryBookStore(seed);
    }

    public IReadOnlyList<Book> List() => _inner.List();
    public Book? GetById(string id) => _inner.GetById(id);
    public void Add(Book item) => _inner.Add(item);
    public void Update(Book item) => _inner.Update(item);
    public bool Remove(string id) => _inner.Remove(id);
    public IReadOnlyList<Book> Search(Func<Book, bool> predicate) => _inner.Search(predicate);
}
