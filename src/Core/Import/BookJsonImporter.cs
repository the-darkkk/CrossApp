using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    public static ImportResult<BookDto> Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            List<BookDto>? rawItems = JsonSerializer.Deserialize<List<BookDto>>(json, options);

            if (rawItems is null || rawItems.Count == 0)
            {
                return new ImportResult<BookDto>([], ["Файл порожній або не містить масиву записів"]);
            }

            var items = new List<BookDto>();
            var errors = new List<string>();

            for (int i = 0; i < rawItems.Count; i++)
            {
                int number = i + 1;
                BookDto? book = rawItems[i];

                string? validationError = Validate(book);

                if (validationError is not null)
                {
                    errors.Add($"елемент {number}: {validationError}");
                }
                else
                {
                    items.Add(book!);
                }
            }

            return new ImportResult<BookDto>(items, errors);
        }
        catch (JsonException ex)
        {
            return new ImportResult<BookDto>([], [$"Синтаксична помилка JSON: {ex.Message}"]);
        }
    }

    private static string? Validate(BookDto? book) => book switch
    {
        null 
            => "порожній об'єкт (null)",

        { Id: null or "" } 
            => "ідентифікатор книги порожній",

        { Isbn: null or "" } or { Title: null or "" } 
            => "ISBN або назва книги порожні",

        { Year: var y } when y < 1450 || y > DateTime.Now.Year 
            => $"рік '{y}' поза допустимими межами (1450-{DateTime.Now.Year})",

        _ => null 
    };
}