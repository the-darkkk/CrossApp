using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

/// <summary>
/// Файлова реалізація сховища книг: той самий контракт IBookStore, інша механіка —
/// кеш у пам'яті, дозавантаження при першому зверненні, запис на диск після кожної зміни.
/// На диск ідуть DTO (BookDto з тижня 3), а мапінг роблять Book.FromDto / book.ToDto:
/// формат файлу не диктує форму домену.
/// </summary>
public sealed class FileBookStore(string path) : IBookStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly Dictionary<string, Book> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    /// <summary>
    /// Ледаче завантаження: зчитує файл лише при першому зверненні.
    /// </summary>
    private void EnsureLoaded()
    {
        if (_loaded) return;
        if (File.Exists(_path))
        {
            var dtos = JsonSerializer.Deserialize<List<BookDto>>(File.ReadAllText(_path)) ?? [];
            foreach (var dto in dtos) { var b = Book.FromDto(dto); _cache[b.Id] = b; }
        }
        _loaded = true;
    }

    /// <summary>
    /// Серіалізація кешу у файл після кожної зміни.
    /// </summary>
    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path,
            JsonSerializer.Serialize(_cache.Values.Select(b => b.ToDto()).ToList(), Options));
    }

    /// <summary>
    /// Повертає список усіх книг з кешу (з попереднім завантаженням з файлу).
    /// </summary>
    public IReadOnlyList<Book> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    /// <summary>
    /// Повертає книгу за id або null.
    /// </summary>
    public Book? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    /// <summary>
    /// Додає книгу з перевіркою унікальності id, після чого записує файл на диск.
    /// </summary>
    public void Add(Book item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
        _cache.Add(item.Id, item);
        Flush();
    }

    /// <summary>
    /// Оновлює книгу у кеші та записує зміни на диск.
    /// </summary>
    public void Update(Book item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    /// <summary>
    /// Видаляє книгу з кешу та записує зміни на диск.
    /// </summary>
    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }

    /// <summary>
    /// Додаткове завдання 2: Пошук з предикатом Func&lt;Book, bool&gt;.
    /// </summary>
    public IReadOnlyList<Book> Search(Func<Book, bool> predicate)
    {
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }
}
