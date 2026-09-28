using Core.Domain;
using Core.Dto;
using Core.Import;

if (args.Length > 0 && !args[0].Equals("--demo", StringComparison.OrdinalIgnoreCase))
{
    string filePath = args[0];
    if (File.Exists(filePath))
    {
        RunCustomImport(filePath);
        return 0;
    }
}

RunLab04Demonstration();
return 0;

static void RunLab04Demonstration()
{
    Console.WriteLine("=== Сценарій 1: успіх ===");

    // 1. Створення каталожної книги
    Book book = Book.Create("B-001", "978-0-13-235088-4", "Clean Code", 2008, "Robert C. Martin");
    Console.WriteLine($"[Створено книгу]: {book}");

    // 2. Реєстрація примірників у захищену колекцію книги
    BookCopy copy1 = book.AddCopy("BC-001");
    BookCopy copy2 = book.AddCopy("BC-002");
    Console.WriteLine($"  -> Додано примірник: {copy1}");
    Console.WriteLine($"  -> Додано примірник: {copy2}");
    Console.WriteLine($"  -> Стан книги: {book}");

    // 3. Створення читача
    Reader reader = Reader.Create("R-001", "Олег Гаргас", "+380501112233", "oleh@example.com");
    Console.WriteLine($"[Створено читача]: {reader}");

    // 4. Оформлення видачі примірника (стан примірника змінюється на IsIssued = true)
    DateTime issueDate = new(2026, 9, 20);
    Loan loan = Loan.Open("L-001", copy1, reader.Id, issueDate, activeReaderLoansCount: 0);
    Console.WriteLine($"[Оформлено видачу]: {loan}");
    Console.WriteLine($"  -> Стан примірника після видачі: {copy1}");

    // 5. Успішне повернення книги (стан примірника повертається в IsIssued = false, видача закривається)
    DateTime returnDate = new(2026, 9, 28);
    loan.Close(returnDate, copy1);
    Console.WriteLine($"[Закрито видачу]: {loan}");
    Console.WriteLine($"  -> Стан примірника після повернення: {copy1}");

    // 6. Демонстрація DTO мапінгу (ToDto / FromDto) для збереження у сховище 5-го тижня
    LoanDto loanDto = loan.ToDto();
    Loan restoredLoan = Loan.FromDto(loanDto);
    Console.WriteLine($"[Відновлено з DTO]: {restoredLoan}");

    Console.WriteLine();
    Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

    BookCopy testCopy = BookCopy.Create("BC-099", "978-0-201-61622-4");
    Loan activeLoan = Loan.Open("L-099", testCopy, "R-001", new DateTime(2026, 9, 20), activeReaderLoansCount: 1);

    Console.WriteLine($"Поточний стан для тестів: {testCopy}");
    Console.WriteLine($"Поточний стан видачі: {activeLoan}\n");

    TryDo("повторна видача вже виданого примірника", () => testCopy.Issue());

    TryDo("відкриття нової видачі на вже зайнятий примірник", () =>
        Loan.Open("L-100", testCopy, "R-002", DateTime.UtcNow, activeReaderLoansCount: 1));

    TryDo("порожній ідентифікатор примірника", () =>
        BookCopy.Create("   ", "978-0-201-61622-4"));

    TryDo("порожній ISBN примірника", () =>
        BookCopy.Create("BC-101", ""));

    TryDo("від'ємний/некоректний рік книги (< 1450)", () =>
        Book.Create("B-999", "978-0-123456-78-9", "Стародавній манускрипт", 1200));

    TryDo("дата повернення раніше дати видачі", () =>
        activeLoan.Close(new DateTime(2026, 9, 10), testCopy));

    TryDo("перевищення ліміту відкритих видач читача (>= 5)", () =>
    {
        var freeCopy = BookCopy.Create("BC-555", "978-0-13-235088-4");
        Loan.Open("L-555", freeCopy, "R-001", DateTime.UtcNow, activeReaderLoansCount: 5);
    });

    activeLoan.Close(new DateTime(2026, 9, 25), testCopy);
    Console.WriteLine($"\n[Видачу L-099 успішно закрито]: стан = {activeLoan.Status}");

    TryDo("повторне закриття закритої видачі (перехід Closed -> Closed)", () =>
        activeLoan.Close(new DateTime(2026, 9, 26), testCopy));

    TryDo("скасування закритої видачі (перехід Closed -> Cancelled)", () =>
        activeLoan.Cancel("Помилковий запис", testCopy));

    TryDo("відновлення FromDto з пошкодженого DTO", () =>
        Loan.FromDto(new LoanDto("L-BAD", "BC-001", "R-001", new DateTime(2026, 9, 20), new DateTime(2026, 9, 15), LoanStatus.Closed)));

    Console.WriteLine("\nСпостережуваний факт: стан об'єктів змінюється виключно через методи;");
    Console.WriteLine("жодна невдала операція не порушила інваріанти моделі.");

    Console.WriteLine();

    Console.WriteLine("=== Додаткове завдання 1: Зв'язок із тижнем 3 (ImportResult -> Сутності) ===");
    string sampleCsvPath = Path.Combine("data", "sample.csv");
    if (File.Exists(sampleCsvPath))
    {
        MixedImportResult rawResult = BookCsvImporter.Load(sampleCsvPath);
        MixedDomainImportResult domainResult = rawResult.ToDomainEntities();

        Console.WriteLine($"Файл: {Path.GetFileName(sampleCsvPath)}");
        Console.WriteLine($"Успішно створено сутностей Book:   {domainResult.Books.Count}");
        Console.WriteLine($"Успішно створено сутностей Reader: {domainResult.Readers.Count}");
        Console.WriteLine($"Загальна кількість помилок:        {domainResult.Errors.Count}");
        if (domainResult.Errors.Count > 0)
        {
            Console.WriteLine("Перелік помилок (синтаксичні + порушення доменних інваріантів):");
            foreach (string err in domainResult.Errors)
            {
                Console.WriteLine($"  ! {err}");
            }
        }
        Console.WriteLine($"СТАТИСТИКА: {domainResult.FormatStats()}");
    }
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

static void RunCustomImport(string filePath)
{
    string ext = Path.GetExtension(filePath).ToLowerInvariant();
    if (ext == ".csv")
    {
        MixedImportResult res = BookCsvImporter.Load(filePath);
        MixedDomainImportResult domainRes = res.ToDomainEntities();
        Console.WriteLine($"=== Імпорт CSV: {domainRes.FormatStats()} ===");
        Console.WriteLine($"Книг: {domainRes.Books.Count}, Читачів: {domainRes.Readers.Count}, Помилок: {domainRes.Errors.Count}");
    }
    else if (ext == ".json")
    {
        ImportResult<BookDto> res = BookJsonImporter.Load(filePath);
        DomainImportResult<Book> domainRes = res.ToDomainEntities();
        Console.WriteLine($"=== Імпорт JSON: {domainRes.FormatStats()} ===");
        Console.WriteLine($"Книг: {domainRes.Items.Count}, Помилок: {domainRes.Errors.Count}");
    }
}