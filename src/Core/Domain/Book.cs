using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Сутність книги в каталозі бібліотеки.
/// Містить захищену колекцію фізичних примірників.
/// </summary>
public sealed class Book
{
    private readonly List<BookCopy> _copies = [];

    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public int Year { get; }
    public string? Author { get; }

    /// <summary>
    /// Захищена колекція примірників (доступ лише для читання назовні).
    /// </summary>
    public IReadOnlyList<BookCopy> Copies => _copies.AsReadOnly();

    private Book(string id, string isbn, string title, int year, string? author)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Year = year;
        Author = author;
    }

    /// <summary>
    /// Фабричний метод для створення сутності книги.
    /// Перевіряє всі інваріанти до виклику конструктора.
    /// </summary>
    public static Book Create(string id, string isbn, string title, int year, string? author = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор книги обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва книги не може бути порожньою", nameof(title));

        int currentYear = DateTime.UtcNow.Year;
        if (year < 1450 || year > currentYear)
        {
            throw new ArgumentOutOfRangeException(
                nameof(year),
                year,
                $"Рік видання ({year}) має бути в межах від 1450 до {currentYear}");
        }

        return new Book(
            id.Trim(),
            isbn.Trim().ToUpperInvariant(),
            title.Trim(),
            year,
            string.IsNullOrWhiteSpace(author) ? null : author.Trim());
    }

    /// <summary>
    /// Реєстрація нового фізичного примірника книги з перевіркою унікальності.
    /// </summary>
    public BookCopy AddCopy(string copyId)
    {
        if (string.IsNullOrWhiteSpace(copyId))
            throw new ArgumentException("Ідентифікатор примірника не може бути порожнім", nameof(copyId));

        string normalizedId = copyId.Trim();
        if (_copies.Any(c => c.Id.Equals(normalizedId, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Примірник з ідентифікатором {normalizedId} вже додано до книги {Id}");
        }

        var copy = BookCopy.Create(normalizedId, Isbn);
        _copies.Add(copy);
        return copy;
    }

    public BookDto ToDto() => new(Id, Isbn, Title, Year, Author);

    public static Book FromDto(BookDto dto) =>
        Create(dto.Id, dto.Isbn, dto.Title, dto.Year, dto.Author);

    public override string ToString() =>
        $"{Id} [{Isbn}] \"{Title}\" ({Year})" +
        (Author is not null ? $" — {Author}" : "") +
        $" | Примірників: {_copies.Count}";
}
