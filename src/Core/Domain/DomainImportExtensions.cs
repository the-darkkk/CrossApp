using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Результат перетворення імпортованих DTO у валідні доменні сутності.
/// Зберігає концепцію лабораторної 3 «дані + помилки».
/// </summary>
public sealed record DomainImportResult<TEntity>(
    IReadOnlyList<TEntity> Items,
    IReadOnlyList<string> Errors)
{
    public int Total => Items.Count + Errors.Count;
    public double ErrorPercentage => Total == 0 ? 0.0 : (double)Errors.Count / Total * 100.0;

    public string FormatStats() =>
        $"Усього: {Total} | Створено сутностей: {Items.Count} | Помилок (парсинг + інваріанти): {Errors.Count} | Відхилено: {ErrorPercentage:F1}%";
}

public sealed record MixedDomainImportResult(
    IReadOnlyList<Book> Books,
    IReadOnlyList<Reader> Readers,
    IReadOnlyList<string> Errors)
{
    public int Accepted => Books.Count + Readers.Count;
    public int Total => Accepted + Errors.Count;
    public double ErrorPercentage => Total == 0 ? 0.0 : (double)Errors.Count / Total * 100.0;

    public string FormatStats() =>
        $"Усього: {Total} | Створено сутностей: {Accepted} (книг: {Books.Count}, читачів: {Readers.Count}) | Помилок: {Errors.Count} | Відхилено: {ErrorPercentage:F1}%";
}

/// <summary>
/// Додаткове завдання 1: Методи перетворення результатів імпорту (ImportResult)
/// у доменні сутності з відсіюванням записів, що порушують інваріанти.
/// </summary>
public static class DomainImportExtensions
{
    public static DomainImportResult<Book> ToDomainEntities(this ImportResult<BookDto> importResult)
    {
        var entities = new List<Book>();
        var errors = new List<string>(importResult.Errors);

        foreach (var dto in importResult.Items)
        {
            try
            {
                entities.Add(Book.FromDto(dto));
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException or InvalidOperationException)
            {
                errors.Add($"Книга DTO [{dto.Id}] (ISBN: {dto.Isbn}): порушення інваріанту — {ex.Message}");
            }
        }

        return new DomainImportResult<Book>(entities.AsReadOnly(), errors.AsReadOnly());
    }

    public static MixedDomainImportResult ToDomainEntities(this MixedImportResult importResult)
    {
        var books = new List<Book>();
        var readers = new List<Reader>();
        var errors = new List<string>(importResult.Errors);

        foreach (var dto in importResult.Books)
        {
            try
            {
                books.Add(Book.FromDto(dto));
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException or InvalidOperationException)
            {
                errors.Add($"Книга DTO [{dto.Id}]: порушення інваріанту — {ex.Message}");
            }
        }

        foreach (var dto in importResult.Readers)
        {
            try
            {
                readers.Add(Reader.FromDto(dto));
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException or InvalidOperationException)
            {
                errors.Add($"Читач DTO [{dto.Id}]: порушення інваріанту — {ex.Message}");
            }
        }

        return new MixedDomainImportResult(books.AsReadOnly(), readers.AsReadOnly(), errors.AsReadOnly());
    }
}
