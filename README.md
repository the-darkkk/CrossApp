# CrossApp — Бібліотечна система

Наскрізний навчальний проєкт з крос-платформного програмування на базі .NET 8 / 10.
Предметна область: «Бібліотека» (`Book`, `BookCopy`, `Reader`, `Loan`).

## Структура рішення

```text
src/
├── Cli/                  # Точка входу (Program.cs — підкоманди звітів)
└── Core/
    ├── Abstractions/     # IBookStore, INotificationSink
    ├── Domain/           # Сутності (Book, BookCopy, Reader, Loan)
    ├── Dto/              # DTO та формати серіалізації
    ├── Events/           # CatalogChangeKind, CatalogChangedEventArgs
    ├── Import/           # CSV/JSON імпортери
    ├── Reports/          # StockLine, GroupSummary, ReportService, LinqExtensions
    ├── Services/         # LendingService, CatalogService
    └── Storage/          # InMemoryBookStore, FileBookStore, CachingBookStore
logs/
└── app.log               # Лог подій
```

## Запуск звітів

```bash
# Підкоманди звітів
dotnet run --project src/Cli -- report-top       # Топ-5 залишків
dotnet run --project src/Cli -- report-low       # Дефіцитні позиції (< 10)
dotnet run --project src/Cli -- report-groups    # Групування за одиницею
dotnet run --project src/Cli -- report-sum       # Агрегатні підсумки
dotnet run --project src/Cli -- report-deferred  # Демо відкладеного виконання
dotnet run --project src/Cli -- report-loans     # Join та GroupJoin
dotnet run --project src/Cli -- report-bench     # Бенчмарк 100 000 записів

# Запуск з файловим сховищем
dotnet run --project src/Cli -- --file report-groups
```

## Звіти LINQ

1. **`LowStock`** — які книги мають дефіцитний залишок (< 10)? (`Where`, `OrderBy`, `ThenBy`, `Select`).
2. **`TopByQuantity`** — які 5 книг мають найбільший залишок? (`OrderByDescending` / `TopBy`, `ThenBy`, `Take`, `Select`).
3. **`ByUnit`** — як розподіляються книги за одиницями/категоріями? (`GroupBy`, `Select`, `Count`, `Sum`, `OrderByDescending`).
4. **`Totals`** — які загальні показники фонду? (`Count`, `Sum`, `Average`, `Max`).



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