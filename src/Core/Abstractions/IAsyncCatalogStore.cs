using Core.Domain;

namespace Core.Abstractions;

public interface IAsyncCatalogStore
{
    Task<IReadOnlyList<Book>> ListAsync(CancellationToken ct = default);
    Task<Book?> GetByIdAsync(string id, CancellationToken ct = default);
    Task AddAsync(Book item, CancellationToken ct = default);
    Task UpdateAsync(Book item, CancellationToken ct = default);
    Task<bool> RemoveAsync(string id, CancellationToken ct = default);
}
