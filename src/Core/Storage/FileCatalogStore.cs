using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

/// <summary>
/// Файлове сховище каталогу з атомарним записом (.tmp + File.Move)
/// та безпечною серіалізацією UTF-8 (тиждень 9).
/// </summary>
public sealed class FileCatalogStore(string path) : ICatalogStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Dictionary<string, Book> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void Flush()
    {
        var dtos = _cache.Values.Select(product => product.ToProductDto()).ToList();
        var tmp = _path + ".tmp";
        string? dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Options));
        File.Move(tmp, _path, overwrite: true);
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;
        if (!File.Exists(_path)) { _loaded = true; return; }

        var text = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(text)) { _loaded = true; return; }

        List<ProductDto>? dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<ProductDto>>(text, Options);
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Помилка десеріалізації JSON у файлі {Path.GetFileName(_path)}: {ex.Message}", ex);
        }

        if (dtos is null)
            throw new DomainException($"Файл {Path.GetFileName(_path)} не містить списку.");

        _cache.Clear();
        for (int i = 0; i < dtos.Count; i++)
        {
            try
            {
                var product = Product.FromDto(dtos[i]);
                _cache[product.Id] = product;
            }
            catch (Exception ex) when (ex is ArgumentException or DomainException)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        _loaded = true;
    }

    public IReadOnlyList<Book> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Book? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Book item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (_cache.ContainsKey(item.Id))
            throw new DomainException($"Запис з id={item.Id} уже існує.");
        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Book item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }

    public IReadOnlyList<Book> Search(Func<Book, bool> predicate)
    {
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }
}
