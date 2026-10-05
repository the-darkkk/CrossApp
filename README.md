# CrossApp — Бібліотечна система

Наскрізний навчальний проєкт з крос-платформного програмування на базі .NET 8 / 10.
Предметна область: «Бібліотека» (`Book`, `BookCopy`, `Reader`, `Loan`).

## Структура рішення

```text
src/
├── Cli/                  # Точка входу (Program.cs — Composition Root, ручний DI)
└── Core/
    ├── Abstractions/     # IBookStore (контракт сховища)
    ├── Domain/           # Сутності (Book, BookCopy, Reader, Loan)
    ├── Dto/              # DTO та формати серіалізації (BookDto, BookCopyDto тощо)
    ├── Import/           # CSV/JSON імпортери
    ├── Services/         # LendingService (бізнес-операції)
    └── Storage/          # InMemoryBookStore, FileBookStore, CachingBookStore, StoreFactory
```

## Запуск

```bash
# Режим у пам'яті (InMemoryBookStore)
dotnet run --project src/Cli

# Режим файлового сховища (FileBookStore, data/library.json)
dotnet run --project src/Cli -- --file
```

## Сервісний шар та сховища

- **Інтерфейс**: `IBookStore` (методи `List`, `GetById`, `Add`, `Update`, `Remove`, `Search`)
- **Реалізації**:
  - `InMemoryBookStore` — словник у пам'яті (`Dictionary<string, Book>`)
  - `FileBookStore` — збереження у файл `data/library.json` через DTO
  - `CachingBookStore` — декоратор із кешуванням списку
- **Сервіс**: `LendingService` — залежить виключно від `IBookStore` (ручний DI через конструктор)
- **Фабрика**: `StoreFactory` — створення сховища за аргументами CLI

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