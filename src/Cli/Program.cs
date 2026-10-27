using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;

// налаштування джерела даних
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
if (!File.Exists(dataPath))
    dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "catalog.json");

IBookStore store = new InMemoryBookStore();
var service = new CatalogService(store);

using var ctsUser = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    ctsUser.Cancel();
};

if (args.Contains("--timeout") || args.Contains("--cancel"))
{
    ctsUser.CancelAfter(TimeSpan.FromMilliseconds(1));
}

using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsUser.Token, ctsTimeout.Token);

if (args.Contains("--corrupt"))
{
    dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "sample.json");
}

// звіт про хід імпорту через IProgress (Додаткове завдання 3)
IProgress<int>? progress = args.Contains("--progress")
    ? new SynchronousProgress(percent => Console.Write($"\rПрогрес: {percent}%   "))
    : null;

// вибір підкоманди або імпорту за замовчуванням
var command = args.FirstOrDefault(a => !a.StartsWith("--"));

try
{
    switch (command)
    {
        case "report-top":
            Print(ReportService.TopByQuantity(SampleData.Books(), 5));
            break;

        case "report-low":
            Print(ReportService.LowStock(SampleData.Books(), 10));
            break;

        case "report-groups":
            foreach (var g in ReportService.ByUnit(SampleData.Books()))
                Console.WriteLine($" {g.Key,-8} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
            break;

        case "report-sum":
            var t = ReportService.Totals(SampleData.Books());
            Console.WriteLine($" позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
            break;

        default:
            var importer = new JsonCatalogImporter();
            var added = await service.ImportAsync(importer, dataPath, progress, cts.Token);
            if (args.Contains("--progress")) Console.WriteLine();
            Console.WriteLine($"Імпортовано записів: {added}");
            break;
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Імпорт скасовано.");
}
catch (DomainException ex)
{
    Console.WriteLine($"Дані відхилено: {ex.Message}");
}
catch (FileNotFoundException)
{
    Console.WriteLine($"Файл не знайдено: {dataPath}");
}

return 0;

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-20} {l.Name,-32} {l.Quantity,4} {l.Unit}");
}

sealed class SynchronousProgress(Action<int> reportAction) : IProgress<int>
{
    public void Report(int value) => reportAction(value);
}