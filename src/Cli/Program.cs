using System.Reflection;
using System.Text.Json;
using Core;
using Core.Configuration;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Microsoft.Extensions.Configuration;

var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();
var legacy = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
var storage = new StorageOptions
{
    DataPath = string.IsNullOrWhiteSpace(legacy) ? options.Storage.DataPath : legacy,
    FileName = options.Storage.FileName
};

var apiKey = config["Integration:ApiKey"];
string dataFilePath = storage.ResolveFullPath(AppContext.BaseDirectory);

Console.WriteLine($"Файл даних : {dataFilePath}");
Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");

var store = new FileCatalogStore(dataFilePath);
var service = new LendingService(store);

var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "export";

try
{
    switch (command)
    {
        case "export":
            if (store.List().Count == 0)
            {
                foreach (var b in SampleData.Books())
                    store.Add(b);
            }
            Console.WriteLine($"Експортовано {store.List().Count} записів.");
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                Console.WriteLine("data/items.json:");
                Console.WriteLine(json.Length > 200 ? json[..200] + "\n..." : json);
            }
            break;

        case "import":
            var items = store.List();
            Console.WriteLine($"Імпортовано {items.Count} записів.");
            break;

        case "report-top":
            Print(ReportService.TopByQuantity(store.List().Count > 0 ? store.List() : SampleData.Books(), 5));
            break;

        case "report-low":
            Print(ReportService.LowStock(store.List().Count > 0 ? store.List() : SampleData.Books(), 10));
            break;

        case "report-groups":
            foreach (var g in ReportService.ByUnit(store.List().Count > 0 ? store.List() : SampleData.Books()))
                Console.WriteLine($" {g.Key,-8} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
            break;

        case "report-sum":
            var t = ReportService.Totals(store.List().Count > 0 ? store.List() : SampleData.Books());
            Console.WriteLine($" позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
            break;

        default:
            Console.WriteLine("export | import | report-top | report-low | report-groups | report-sum");
            break;
    }
}
catch (JsonException ex)
{
    Console.WriteLine($"Помилка читання JSON: {ex.Message}");
}
catch (DomainException ex)
{
    Console.WriteLine($"Дані відхилено: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

return 0;

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-20} {l.Name,-32} {l.Quantity,4} {l.Unit}");
}
