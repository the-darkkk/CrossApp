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
            List<BookDto>? items = JsonSerializer.Deserialize<List<BookDto>>(json, options);

            return new ImportResult<BookDto>(items ?? [], []);
        }
        catch (JsonException ex)
        {
            return new ImportResult<BookDto>([], [$"Помилка валідації JSON: {ex.Message}"]);
        }
    }
}