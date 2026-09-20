using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

return extension switch
{
    ".csv" => ProcessCsv(path),
    ".json" => ProcessJson(path),
    _ => HandleUnknownExtension(extension)
};

static int ProcessCsv(string filePath)
{
    MixedImportResult result = BookCsvImporter.Load(filePath);

    Console.WriteLine($"=== Результат імпорту CSV ({Path.GetFileName(filePath)}) ===");
    Console.WriteLine($"Книг знайдено: {result.Books.Count}");
    foreach (BookDto b in result.Books.Take(3))
        Console.WriteLine($" [Книга]  {b.Id,-6} {b.Isbn,-18} {b.Title,-28} {b.Year,4}");

    Console.WriteLine($"Читачів знайдено: {result.Readers.Count}");
    foreach (ReaderDto r in result.Readers.Take(3))
        Console.WriteLine($" [Читач]  {r.Id,-6} {r.FullName,-22} {r.Phone}");

    if (result.Errors.Count > 0)
    {
        Console.WriteLine($"Помилки ({result.Errors.Count}):");
        foreach (string err in result.Errors)
            Console.WriteLine($" ! {err}");
    }

    Console.WriteLine();
    Console.WriteLine($"СТАТИСТИКА: {result.FormatStats()}");
    return 0;
}

static int ProcessJson(string filePath)
{
    ImportResult<BookDto> result = BookJsonImporter.Load(filePath);

    Console.WriteLine($"=== Результат імпорту JSON ({Path.GetFileName(filePath)}) ===");
    Console.WriteLine($"Завантажено записів: {result.Items.Count}");
    foreach (BookDto b in result.Items.Take(5))
        Console.WriteLine($" {b.Id,-6} {b.Isbn,-18} {b.Title,-28} {b.Year,4}");

    if (result.Errors.Count > 0)
    {
        Console.WriteLine($"Помилки:");
        foreach (string err in result.Errors)
            Console.WriteLine($" ! {err}");
    }

    Console.WriteLine();
    Console.WriteLine($"СТАТИСТИКА: {result.FormatStats()}");
    return 0;
}

static int HandleUnknownExtension(string ext)
{
    Console.WriteLine($"Помилка: непідтримуваний формат файлу '{ext}'. Очікується .csv або .json.");
    return 1;
}