using Cli;
using Core;
using Core.Abstractions;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

// вибір сховища
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "library.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Books());

var service = new LendingService(store);

// шлях до логу
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
if (File.Exists(logPath))
    File.Delete(logPath);

// перший підписник: локальний метод
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var details = e.ReaderId != null
        ? (e.Kind == CatalogChangeKind.CopyIssued
            ? $" (видано читачу {e.ReaderId} до {e.DueDate:dd.MM.yyyy})"
            : $" (повернуто від читача {e.ReaderId})")
        : "";
    var line = $"{e.At:HH:mm:ss} {e.Kind,-12} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}{details}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged;

// другий підписник: лямбда із замиканням
int changes = 0;
service.Changed += (_, _) => changes++;

// додавання та зміна стану
var created = service.AddBook("978-0-13-235088-4", "Clean Code", 2008, "Robert C. Martin", initialCopies: 2);
var copy = created.Copies[0];
var loan = service.IssueCopy(created.Id, copy.Id, "R-001");
Console.WriteLine($"Подій отримано: {changes}");

// відписка: тиха операція
service.Changed -= OnCatalogChanged;
service.ReturnCopy(created.Id, copy.Id, loan);
Console.WriteLine($"Подій отримано: {changes}");

// сценарій відмови
try
{
    service.Receive(created.Id, -5);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Помилка валідації: {ex.ParamName} ({ex.Message})");
}

try
{
    service.IssueCopy("НЕІСНУЮЧИЙ-ID", "BC-999", "R-001");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Помилка інваріанту: {ex.Message}");
}

// пошук через Func
Console.WriteLine("\nКниги з роком видання до 2010 (FindAll):");
var filtered = service.FindAll(b => b.Year < 2010);
foreach (var b in filtered)
    Console.WriteLine($"  {b.Id,-8} {b.Title,-30} ({b.Year}) | Примірників: {b.Copies.Count}");

// додаткове завдання 1: INotificationSink
Console.WriteLine("\nДодаткове завдання 1 (INotificationSink):");
var sinkLog = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");
var consoleSink = new ConsoleSink();
var fileSink = new FileSink(sinkLog);
var sinkService = new LendingService(store, consoleSink);
sinkService.AddBook("978-0-12-345678-9", "Книга через Sink", 2023, "Автор", initialCopies: 1);
fileSink.Emit("Прямий запис у FileSink");

// додаткове завдання 2: Action<string> для імпорту
Console.WriteLine("\nДодаткове завдання 2 (Action<string> для пропущених рядків):");
string csvPath = Path.Combine(AppContext.BaseDirectory, "data", "sample.csv");
if (!File.Exists(csvPath))
    csvPath = "data/sample.csv";

var importResult = BookCsvImporter.Load(csvPath, msg => Console.WriteLine($"  [Пропущено]: {msg}"));
Console.WriteLine($"Успішно імпортовано книг: {importResult.Books.Count}, читачів: {importResult.Readers.Count}");

return 0;