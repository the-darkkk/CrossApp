using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

/// <summary>
/// Асинхронний імпортер книг із JSON-файлу через неблокувальний FileStream (тиждень 8).
/// </summary>
public sealed class JsonCatalogImporter : ICatalogImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Асинхронно завантажує книги з файлу за допомогою справжнього неблокувального вводу-виводу.
    /// </summary>
    public async Task<IReadOnlyList<Book>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        List<BookDto>? dtos;
        try
        {
            dtos = await JsonSerializer.DeserializeAsync<List<BookDto>>(
                stream,
                JsonOptions,
                cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Помилка десеріалізації JSON у файлі {Path.GetFileName(path)}", ex);
        }

        if (dtos is null)
            throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        var result = new List<Book>(dtos.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                result.Add(Book.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }

    /// <summary>
    /// Потокове асинхронне читання через IAsyncEnumerable з атрибутом [EnumeratorCancellation] (Додаткове завдання 1).
    /// </summary>
    public async IAsyncEnumerable<Book> ReadAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        int index = 0;
        await foreach (var dto in JsonSerializer.DeserializeAsyncEnumerable<BookDto>(stream, JsonOptions, cancellationToken: ct))
        {
            ct.ThrowIfCancellationRequested();
            if (dto is null) continue;
            index++;

            Book book;
            try
            {
                book = Book.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {index}: {ex.Message}", ex);
            }

            yield return book;
        }
    }
}

