using Core.Domain;

namespace Core.Abstractions;

/// <summary>
/// Асинхронний контракт імпортера каталогу книг (тиждень 8).
/// </summary>
public interface ICatalogImporter
{
    /// <summary>
    /// Асинхронне завантаження повного списку книг із файлу з підтримкою скасування.
    /// </summary>
    Task<IReadOnlyList<Book>> LoadAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Потокове асинхронне читання через IAsyncEnumerable (Додаткове завдання 1).
    /// </summary>
    IAsyncEnumerable<Book> ReadAsync(string path, CancellationToken ct = default);
}

