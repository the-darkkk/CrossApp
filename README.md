# CrossApp — Бібліотечна система

Наскрізний навчальний проєкт з крос-платформного програмування на базі .NET 8 / 10.
Предметна область: «Бібліотека» (`Book`, `BookCopy`, `Reader`, `Loan`).

## Структура рішення

```text
src/
├── Cli/                  # Консольний клієнт (топ-левел await, обробка Ctrl+C, прогрес)
└── Core/
    ├── Abstractions/     # IBookStore, ICatalogImporter, INotificationSink
    ├── Domain/           # Сутності (Book, BookCopy, Reader, Loan)
    ├── DomainException.cs# Доменний виняток порушення бізнес-правил
    ├── Dto/              # DTO записів
    ├── Events/           # CatalogChangeKind, CatalogChangedEventArgs
    ├── Reports/          # StockLine, GroupSummary, ReportService, LinqExtensions
    ├── Services/         # LendingService, CatalogService
    └── Storage/          # JsonCatalogImporter, InMemoryBookStore, FileBookStore
tests/
└── Core.Tests/           # xUnit тести для Core (без посилання на Cli)
```

## Запуск та перевірка

```bash
# Асинхронний імпорт каталогу
dotnet run --project src/Cli -- --file

# Перевірка скасування (таймаут або Ctrl+C)
dotnet run --project src/Cli -- --timeout

# Перевірка виявлення некоректних даних (DomainException)
dotnet run --project src/Cli -- --corrupt

# Запуск з відображенням прогресу IProgress
dotnet run --project src/Cli -- --file --progress

# Запуск усіх unit-тестів
dotnet test

# Збір покриття коду
dotnet test --collect:"XPlat Code Coverage"
```

## Асинхронність та доменні винятки

- **Асинхронні операції**: операції файлового введення-виведення (`LoadAsync`, `ReadAsync`, `ImportAsync`), які використовують неблокувальний `FileStream(useAsync: true)` та підтримку `CancellationToken`. Чисті обчислення (доменні правила, LINQ-звіти, операції в пам'яті) залишені синхронними, щоб не створювати зайвий оверхед.
- **DomainException vs ArgumentException**:
  - `ArgumentException` / `ArgumentOutOfRangeException` перевіряють формат аргументів методу (порожні поля, некоректні межі року або від'ємна кількість).
  - `DomainException` сигналізує про порушення бізнес-правил стану сутностей (повторна видача вже виданого примірника, спроба повернути невидану книгу, перевищення ліміту відкритих видач читача).

### Фрагменти коду

**Асинхронне читання з перевіркою скасування (`LoadAsync`):**
```csharp
await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
var dtos = await JsonSerializer.DeserializeAsync<List<BookDto>>(stream, JsonOptions, cancellationToken: ct)
    ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

for (var i = 0; i < dtos.Count; i++)
{
    ct.ThrowIfCancellationRequested();
    try { result.Add(Book.FromDto(dtos[i])); }
    catch (ArgumentException ex) { throw new DomainException($"Запис {i + 1}: {ex.Message}", ex); }
}
```

**Обробка скасування в Cli:**
```csharp
try
{
    var added = await service.ImportAsync(importer, dataPath, progress, cts.Token);
    Console.WriteLine($"Імпортовано записів: {added}");
}
catch (OperationCanceledException) { Console.WriteLine("Імпорт скасовано."); }
catch (DomainException ex) { Console.WriteLine($"Дані відхилено: {ex.Message}"); }
```

## Тести (xUnit)

| Тест | Що перевіряє | Очікуваний результат |
|---|---|---|
| `Receive_WithNegativeQuantity_Throws_AndKeepsState` | Інваріант аргументу кількості | `ArgumentOutOfRangeException`, стан не змінився |
| `Add_RaisesChangedExactlyOnce` | Підняття події `Changed` | Рівно одна подія з типом `BookAdded` |
| `ReturnCopy_WhenCopyNotIssued_ThrowsDomainException` | Інваріант повернення книги | `DomainException` («не був виданий»), статус не змінено |
| `IssueCopy_WhenAlreadyIssued_ThrowsDomainException` | Інваріант повторної видачі | `DomainException` («вже виданий») |
| `LowStock_CountsOnlyBelowThreshold` | Звіт `LowStock` на різних порогах | Точна кількість записів нижче порогу |
| `TopByQuantity_IsDeterministic` | Стабільність сортування топ-N | Детермінований порядок за primary + secondary ключем |
| `Totals_OnEmptyCollection_DoesNotThrow` | Обробка порожнього джерела | Без винятків, нульові показники `(0, 0, 0d, 0)` |
| `ImportAsync_WhenAlreadyCancelled_Throws` | Перевірка попередньо скасованого токена | `OperationCanceledException` |
| `ImportAsync_WithCorruptData_ThrowsDomainException` | Реакція на некоректні дані у файлі | `DomainException` із номером запису, файл видалено у `finally` |
| `ImportAsync_WithLinkedToken_ThrowsOnTimeout` | Спрацювання таймауту у зв'язаних токенах | Скасовано таймаут-токеном, користувацький токен чистий |
| `ImportStreamAsync_ImportsItemsSuccessfully` | Потоковий імпорт через `IAsyncEnumerable` | Усі записи імпортовано |
| `ImportAsync_ReportsProgress` | Звіт прогресу `IProgress<int>` | Прогрес досягає 100% |

## Додаткові завдання

- **IAsyncEnumerable**: у `JsonCatalogImporter.ReadAsync` реалізовано потоковий імпорт з атрибутом `[EnumeratorCancellation]`, що читає дані через `JsonSerializer.DeserializeAsyncEnumerable` без завантаження всього масиву в пам'ять.
- **Зв'язані токени**: перевірено створення `CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutToken)` та протестовано, що спрацьовує саме таймаут.
- **IProgress<int>**: додано звітування прогресу у відсотках під час імпорту.
- **Покриття коду**: покрито доменні сутності, сервіс, звіти та імпорт. Непокритими залишились дискові сховища минулих робіт (`FileBookStore`) та генератор `SampleData`, оскільки unit-тести згідно з вимогами не взаємодіють з реальною файловою системою.
