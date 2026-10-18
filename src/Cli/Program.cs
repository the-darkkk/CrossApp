using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;

// вибір сховища (тиждень 5)
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "library.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Books());

var service = new LendingService(store);
var items = service.All();

// підкоманди звітів (тиждень 7)
var command = args.FirstOrDefault(a => !a.StartsWith("--"));

switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Print(ReportService.LowStock(items, 10));
        break;

    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
            Console.WriteLine($" {g.Key,-8} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        break;

    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($" позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;

    case "report-deferred":
        RunDeferredDemo(service);
        break;

    case "report-loans":
        Console.WriteLine("Топ читачів за видачами (GroupJoin):");
        var readers = SampleData.Readers();
        var loans = SampleData.Loans(items);
        foreach (var r in ReportService.TopReadersByLoans(readers, loans, 5))
            Console.WriteLine($" {r.ReaderId,-6} {r.ReaderName,-16} видач: {r.TotalLoans,2} (активних: {r.ActiveLoans})");

        Console.WriteLine("\nАктивні видачі з назвами книг (Join):");
        foreach (var l in ReportService.ActiveLoansWithBooks(loans, items))
            Console.WriteLine($" {l.LoanId,-6} {l.BookTitle,-28} примірник: {l.CopyId,-12} читач: {l.ReaderId}");
        break;

    case "report-bench":
        Console.WriteLine("Бенчмарк фільтрації на 100 000 записах (5 повторів):");
        var (withoutToList, withToList) = ReportService.BenchmarkFiltering(100_000);
        Console.WriteLine($" Без проміжного ToList: {withoutToList} ms");
        Console.WriteLine($" З проміжним ToList:    {withToList} ms");
        break;

    case null:
        // запуск за замовчуванням: демонстрація відкладеного виконання та топ-звіт
        RunDeferredDemo(service);
        Console.WriteLine();
        Print(ReportService.TopByQuantity(items, 5));
        break;

    default:
        Console.WriteLine("report-top | report-low | report-groups | report-sum | report-deferred | report-loans | report-bench");
        break;
}

return 0;

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-20} {l.Name,-32} {l.Quantity,4} {l.Unit}");
}

static void RunDeferredDemo(LendingService svc)
{
    var source = svc.All().ToList();
    var query = ReportService.QueryLowStock(source, 10);
    Console.WriteLine($"Дефіцитних до змін: {query.Count()}");
    source.Add(Book.Create("tmp00001", "978-0-99-999999-9", "Скоба", 2024, unit: "шт"));
    Console.WriteLine($"Дефіцитних після змін: {query.Count()}");
    var snapshot = query.ToList();
    source.Clear();
    Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
}