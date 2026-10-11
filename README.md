# CrossApp — Бібліотечна система

Наскрізний навчальний проєкт з крос-платформного програмування на базі .NET 8 / 10.
Предметна область: «Бібліотека» (`Book`, `BookCopy`, `Reader`, `Loan`).

## Структура рішення

```text
src/
├── Cli/                  # Точка входу (Program.cs, Sinks.cs)
└── Core/
    ├── Abstractions/     # IBookStore, INotificationSink
    ├── Domain/           # Сутності (Book, BookCopy, Reader, Loan)
    ├── Dto/              # DTO та формати серіалізації
    ├── Events/           # CatalogChangeKind, CatalogChangedEventArgs
    ├── Import/           # CSV/JSON імпортери
    ├── Services/         # LendingService, CatalogService
    └── Storage/          # InMemoryBookStore, FileBookStore, CachingBookStore
logs/
└── app.log               # Лог подій
```

## Запуск

```bash
# Режим у пам'яті
dotnet run --project src/Cli

# Режим файлового сховища
dotnet run --project src/Cli -- --file
```

## Доменні події

- **Події**: `BookAdded`, `CopyIssued`, `CopyReturned` — позначають успішну зміну фонду або статусу видачі примірника.
- **Оголошення та OnChanged**:
```csharp
public event EventHandler<CatalogChangedEventArgs>? Changed;

private void OnChanged(Book p, CatalogChangeKind kind, int delta, string? copyId = null, string? readerId = null, DateTimeOffset? dueDate = null)
{
    int available = p.Copies.Count(c => !c.IsIssued);
    Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, available, copyId, readerId, dueDate));
    _sink?.Emit($"{kind}: {p.Id} ({p.Title})");
}
```

## Доменні інваріанти

| № | Правило (інваріант) | Опис обмеження | Виняток | Метод |
|:---:|---|---|---|---|
| 1 | Валідація обов'язкових ключів | `Id`, `Isbn`, `Title`, `FullName` не можуть бути порожніми | `ArgumentException` | Фабричні методи `Create` / `Open` |
| 2 | Межі року видання | Рік книги повинен бути в діапазоні від 1450 до поточного року | `ArgumentOutOfRangeException` | `Book.Create` |
| 3 | Унікальність примірників | Заборонено додавати примірник із дубльованим `Id` у межах однієї книги | `InvalidOperationException` | `Book.AddCopy` |
| 4 | Захист від повторної видачі | Неможливо видати примірник, який уже має стан `IsIssued == true` | `InvalidOperationException` | `BookCopy.Issue`, `Loan.Open` |
| 5 | Захист від некоректного повернення | Неможливо повернути примірник, який не значиться виданим | `InvalidOperationException` | `BookCopy.Return`, `Loan.Close` |
| 6 | Хронологія дат видачі | Дата повернення книги не може передувати даті видачі | `ArgumentOutOfRangeException` | `Loan.Close`, `Loan.FromDto` |
| 7 | Ліміт видач читача (2 сутності) | Заборонено оформляти видачу, якщо читач уже має >= 5 відкритих видач | `InvalidOperationException` | `Loan.Open` |
| 8 | Контроль переходів станів | Дозволені лише переходи `Active -> Closed` та `Active -> Cancelled` | `InvalidOperationException` | `Loan.TransitionTo` |