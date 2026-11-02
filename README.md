# CrossApp — Бібліотечна система

Наскрізний навчальний проєкт з крос-платформного програмування на базі .NET 8 / 10.
Предметна область: «Бібліотека» (`Book`, `BookCopy`, `Reader`, `Loan`).

## Структура рішення

```text
src/
├── Cli/                  # Точка входу, конфігурація, appsettings.json, appsettings.Development.json
└── Core/
    ├── Abstractions/     # IBookStore, ICatalogStore, ICatalogImporter, INotificationSink
    ├── Configuration/    # AppOptions, StorageOptions, LoggingOptions, LibraryOptions
    ├── Domain/           # Book, BookCopy, Reader, Loan, Product
    ├── DomainException.cs# Доменні винятки бізнес-правил
    ├── Dto/              # BookDto, ProductDto, LoanDto тощо
    ├── Events/           # CatalogChangeKind, CatalogChangedEventArgs
    ├── Reports/          # StockLine, GroupSummary, ReportService, LinqExtensions
    ├── Services/         # LendingService, CatalogService
    └── Storage/          # FileCatalogStore, FileBookStore, InMemoryCatalogStore, JsonCatalogImporter
tests/
└── Core.Tests/           # xUnit тести
```

## Запуск та конфігурація

```bash
# Експорт колекції у JSON
dotnet run --project src/Cli -- export

# Імпорт колекції з JSON
dotnet run --project src/Cli -- import

# Перевизначення шляху до даних (PowerShell)
$env:CROSSAPP_DATA_PATH = "custom_data"
dotnet run --project src/Cli -- export
Remove-Item Env:\CROSSAPP_DATA_PATH

# Перевизначення вкладеного ключа (PowerShell)
$env:CROSSAPP_Storage__FileName = "catalog.json"
dotnet run --project src/Cli -- export
Remove-Item Env:\CROSSAPP_Storage__FileName

# Приклад для bash:
# CROSSAPP_DATA_PATH=/tmp/data dotnet run --project src/Cli -- export
# CROSSAPP_Integration__ApiKey=prod-key dotnet run --project src/Cli -- export

# Запуск тестів
dotnet test
```

## Таблиця ключів конфігурації

| Ключ конфігурації | Змінна середовища | За замовчуванням | Опис |
|---|---|---|---|
| `EnvironmentName` | `CROSSAPP_ENVIRONMENT` | `Development` | Профіль середовища (`Development` / `Production`) |
| `Storage:DataPath` | `CROSSAPP_Storage__DataPath` або `CROSSAPP_DATA_PATH` | `data` | Каталог розміщення файлів даних |
| `Storage:FileName` | `CROSSAPP_Storage__FileName` | `items.json` | Назва файлу каталогу |
| `Logging:MinimumLevel` | `CROSSAPP_Logging__MinimumLevel` | `Information` (dev: `Debug`) | Рівень деталізації логів |
| `Library:LoanDays` | `CROSSAPP_Library__LoanDays` | `14` (dev: `21`) | Термін видачі книги у днях |
| `Integration:ApiKey` | `CROSSAPP_Integration__ApiKey` | `local-dev-key` (user-secrets) | API-ключ інтеграції (поза репозиторієм) |

## Особливості реалізації

1. **Профілі `Development` та `Production`**:
   - `appsettings.json` містить базові налаштування (рівень логування `Information`, термін видачі 14 днів).
   - `appsettings.Development.json` перекриває параметри для розробки (рівень `Debug`, термін видачі 21 день).
   - Змінна `CROSSAPP_ENVIRONMENT` визначає активний профіль.

2. **Кросплатформні шляхи**:
   - Для побудови шляхів використано `Path.Combine`, який підставляє коректний системний роздільник (`\` для Windows, `/` для Linux/macOS).
   - Відносний шлях `DataPath` резолвиться від `AppContext.BaseDirectory`, завдяки чому файл даних однаковий незалежно від того, з якої поточної папки (`cwd`) запущено програму.

3. **Атомарний запис JSON**:
   - У `FileCatalogStore` дані спочатку записуються в тимчасовий файл `items.json.tmp`, після чого замінюють основний через `File.Move(tmp, path, overwrite: true)`. Це гарантує цілісність даних при раптових збоях.
   - Використано `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`, щоб українські літери записувалися як UTF-8 символи, а не екранувалися в `\uXXXX`.

4. **User-secrets**:
   - Зберігаються поза репозиторієм: на Windows у `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`.
   - У `Cli.csproj` міститься лише ідентифікатор `<UserSecretsId>`, ключ `Integration:ApiKey` у Git не потрапляє. У консоль виводиться лише факт присутності та довжина ключа.

## Доменні інваріанти

| № | Правило (інваріант) | Опис обмеження | Виняток | Метод |
|:---:|---|---|---|---|
| 1 | Валідація обов'язкових ключів | `Id`, `Isbn`, `Title`, `FullName` не можуть бути порожніми | `ArgumentException` | Фабричні методи `Create` / `Open` |
| 2 | Межі року видання | Рік книги повинен бути в діапазоні від 1450 до поточного року | `ArgumentOutOfRangeException` | `Book.Create` |
| 3 | Унікальність примірників | Заборонено додавати примірник із дубльованим `Id` у межах однієї книги | `DomainException` | `Book.AddCopy` |
| 4 | Захист від повторної видачі | Неможливо видати примірник, який уже має стан `IsIssued == true` | `DomainException` | `BookCopy.Issue`, `Loan.Open` |
| 5 | Захист від некоректного повернення | Неможливо повернути примірник, який не значиться виданим | `DomainException` | `BookCopy.Return`, `Loan.Close` |
| 6 | Хронологія дат видачі | Дата повернення книги не може передувати даті видачі | `ArgumentOutOfRangeException` | `Loan.Close`, `Loan.FromDto` |
| 7 | Ліміт видач читача (2 сутності) | Заборонено оформляти видачу, якщо читач уже має >= 5 відкритих видач | `DomainException` | `Loan.Open` |
| 8 | Контроль переходів станів | Дозволені лише переходи `Active -> Closed` та `Active -> Cancelled` | `DomainException` | `Loan.TransitionTo` |
