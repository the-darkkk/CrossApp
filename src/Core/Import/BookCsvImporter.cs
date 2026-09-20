using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class BookCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var books = new List<BookDto>();
        var readers = new List<ReaderDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропуск рядка заголовків
            if (number == 1 && (line.StartsWith("type", StringComparison.OrdinalIgnoreCase) || line.StartsWith("id", StringComparison.OrdinalIgnoreCase)))
                continue;

            switch (ParseLine(line))
            {
                case BookOk book:
                    books.Add(book.Value);
                    break;
                case ReaderOk reader:
                    readers.Add(reader.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new MixedImportResult(books, readers, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 } 
                => new ParseFailed($"замало колонок: {parts.Length}"),

            ["B", _, "", _, _] or ["B", _, _, "", _] 
                => new ParseFailed("ISBN або назва книги порожні"),

            ["B", _, _, _, var year] when !int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) || y < 1450 || y > DateTime.Now.Year 
                => new ParseFailed($"рік книги '{year}' некоректний (1450-{DateTime.Now.Year})"),

            ["B", var id, var isbn, var title, var year] 
                => new BookOk(new BookDto(id, isbn, title, int.Parse(year, CultureInfo.InvariantCulture))),

            [var id, var isbn, var title, var year] when parts[0] != "R" && int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y)
                => new BookOk(new BookDto(id, isbn, title, y)),

            ["R", _, "", _] or ["R", _, _, ""] 
                => new ParseFailed("ПІБ або телефон читача порожні"),

            ["R", var id, var name, var phone] 
                => new ReaderOk(new ReaderDto(id, name, phone)),

            [var prefix, ..] when prefix != "B" && prefix != "R" 
                => new ParseFailed($"невідомий тип запису '{prefix}' (очікується 'B' або 'R')"),

            _ => new ParseFailed($"невідповідна кількість колонок для цього типу ({parts.Length})")
        };
    }

    private abstract record ParseOutcome;
    private sealed record BookOk(BookDto Value) : ParseOutcome;
    private sealed record ReaderOk(ReaderDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}