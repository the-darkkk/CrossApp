using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

// вибір сховища через аргумент --file
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "library.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Books());

var service = new LendingService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine($"Шлях: {dataPath}\n");

// додавання книги, реєстрація примірника, видача та повернення
Book created = service.AddBook("978-3-16-148410-0", "Нова книга з каталогу", 2024, "Автор Новий");
Console.WriteLine($"[Додано книгу]: {created}");

BookCopy newCopy = service.RegisterCopy(created.Id, "BC-NEW-001");
Console.WriteLine($"[Зареєстровано примірник]: {newCopy}");

Loan loan = service.IssueCopy(created.Id, newCopy.Id, "R-001");
Console.WriteLine($"[Видано примірник]: {loan}");

service.ReturnCopy(created.Id, newCopy.Id, loan);
Console.WriteLine($"[Повернуто примірник]: {loan}\n");

// список усіх книг
Console.WriteLine("Список усіх книг:");
foreach (Book b in service.All())
    Console.WriteLine($"  {b.Id,-8} {b.Isbn,-22} {b.Title,-30} {b.Year,4} | Примірників: {b.Copies.Count}");

Console.WriteLine($"\nУсього книг у каталозі: {service.All().Count}\n");

// пошук за id
Console.WriteLine("Пошук за id:");
Book? found = service.Find(created.Id);
Console.WriteLine(found is not null ? $"[Знайдено]: {found}" : $"[Не знайдено]: id={created.Id}");

Book? notFound = service.Find("НЕІСНУЮЧИЙ-ID");
Console.WriteLine(notFound is not null ? $"[Знайдено]: {notFound}" : "[Не знайдено]: id=НЕІСНУЮЧИЙ-ID\n");

// пошук за предикатом
Console.WriteLine("Пошук за предикатом (від 2024 року):");
IReadOnlyList<Book> recentBooks = service.Search(b => b.Year >= 2024);
foreach (Book b in recentBooks)
    Console.WriteLine($"  {b.Id,-8} {b.Title,-30} ({b.Year})");
Console.WriteLine($"Знайдено: {recentBooks.Count}\n");

// видалення книги
Console.WriteLine("Видалення книги:");
Book tempBook = service.AddBook("978-0-00-999999-9", "Тимчасова книга", 2020);
bool removed = service.Remove(tempBook.Id);
Console.WriteLine($"[Видалено тимчасову id={tempBook.Id}]: {removed}");
Console.WriteLine($"Кількість книг у каталозі: {service.All().Count}\n");

// обробка помилок
Console.WriteLine("Обробка помилок:");

// дубль id
try
{
    service.AddBook(created.Id, "978-0-13-235088-4", "Дублікат", 2024);
    Console.WriteLine("  дубль id: виняток не спрацював");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  дубль id: {ex.Message}");
}

// неіснуючий id
try
{
    service.RegisterCopy("НЕІСНУЮЧИЙ-ID", "BC-999");
    Console.WriteLine("  неіснуючий id: виняток не спрацював");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  неіснуючий id: {ex.Message}");
}

// повторна видача
try
{
    Loan testLoan = service.IssueCopy(created.Id, newCopy.Id, "R-002");
    try
    {
        service.IssueCopy(created.Id, newCopy.Id, "R-003");
        Console.WriteLine("  повторна видача: виняток не спрацював");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"  повторна видача: {ex.Message}");
    }
    finally
    {
        service.ReturnCopy(created.Id, newCopy.Id, testLoan);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  помилка тесту видачі: {ex.Message}");
}

Console.WriteLine();

// декоратор CachingBookStore
Console.WriteLine("Декоратор CachingBookStore:");
var innerStore = new InMemoryBookStore(SampleData.Books());
var cachingStore = new CachingBookStore(innerStore);
var cachingService = new LendingService(cachingStore);

Console.WriteLine($"Книг у кеші (до додавання): {cachingService.All().Count}");
Book cachedBook = cachingService.AddBook("978-0-00-000000-0", "Кешована книга", 2025);
Console.WriteLine($"[Додано через декоратор]: {cachedBook.Id}");
Console.WriteLine($"Книг у кеші (після скидання): {cachingService.All().Count}\n");

// фабрика StoreFactory
Console.WriteLine("Фабрика StoreFactory:");
IBookStore factoryStore = StoreFactory.Create(args, dataPath, SampleData.Books());
Console.WriteLine($"Створено тип: {factoryStore.GetType().Name}");
Console.WriteLine($"Книг у сховищі: {factoryStore.List().Count}\n");

return 0;