using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Аліас/фабрика для доменної сутності книги під універсальну назву Product.
/// </summary>
public static class Product
{
    public static Book Create(string id, string sku, string name, string unit, int quantity)
    {
        var book = Book.Create(id, sku, name, DateTime.UtcNow.Year, unit: unit);
        for (int i = 1; i <= quantity; i++)
        {
            book.AddCopy($"BC-{id}-{i:D2}");
        }
        return book;
    }

    public static Book Create(string id, string sku, string name, int year, string? author = null, string unit = "прим.", string? genre = null) =>
        Book.Create(id, sku, name, year, author, unit, genre);

    public static Book FromDto(ProductDto dto) =>
        Book.FromProductDto(dto);

    public static Book FromDto(BookDto dto) =>
        Book.FromDto(dto);
}

