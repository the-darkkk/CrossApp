using Core.Domain;

namespace Core.Reports;

/// <summary>
/// Сервіс аналітичних звітів через LINQ to Objects.
/// Всі методи є чистими функціями без залежності від I/O та консолі.
/// </summary>
public static class ReportService
{
    /// <summary>
    /// Фільтр дефіцитних позицій: Where + OrderBy/ThenBy + Select (проєкція в DTO).
    /// </summary>
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Book> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    /// <summary>
    /// Топ-N позицій за залишком з детермінованим сортуванням (Додаткове завдання 2: розширення TopBy).
    /// </summary>
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Book> source, int n) =>
        source.TopBy(p => p.Quantity, p => p.Sku, n)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    /// <summary>
    /// Групування за одиницею виміру або категорією: GroupBy + агрегати всередині групи.
    /// </summary>
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Book> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
            .OrderByDescending(s => s.TotalQuantity)
            .ToList();

    /// <summary>
    /// Звіт групування у синтаксисі запитів query comprehension (Додаткове завдання 3).
    /// </summary>
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Book> source)
    {
        var query = from p in source
                    group p by p.Unit into g
                    orderby g.Sum(x => x.Quantity) descending
                    select new GroupSummary(g.Key, g.Count(), g.Sum(x => x.Quantity));

        return query.ToList();
    }

    /// <summary>
    /// Агрегатні підсумки з безпечною обробкою порожньої послідовності.
    /// </summary>
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Book> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity),
               list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    /// <summary>
    /// Відкладений запит фільтрації для демонстрації відкладеного виконання.
    /// </summary>
    public static IEnumerable<Book> QueryLowStock(IEnumerable<Book> source, int threshold) =>
        source.Where(p => p.Quantity < threshold);

    /// <summary>
    /// Зведений звіт за читачами та видачами: GroupJoin (Додаткове завдання 1 та 2).
    /// </summary>
    public static IReadOnlyList<ReaderLoanSummary> TopReadersByLoans(
        IEnumerable<Reader> readers,
        IEnumerable<Loan> loans,
        int n)
    {
        var summaries = readers.GroupJoin(
            loans,
            r => r.Id,
            l => l.ReaderId,
            (r, rLoans) => new ReaderLoanSummary(
                r.Id,
                r.FullName,
                rLoans.Count(),
                rLoans.Count(l => l.Status == LoanStatus.Active)))
            .ToList();

        return summaries.TopBy(s => s.TotalLoans, s => s.ReaderId, n);
    }

    /// <summary>
    /// З'єднання видач із книгами: Join через спільний ідентифікатор примірника (Додаткове завдання 1).
    /// </summary>
    public static IReadOnlyList<BookLoanLine> ActiveLoansWithBooks(
        IEnumerable<Loan> loans,
        IEnumerable<Book> books)
    {
        var copyToBook = books.SelectMany(b => b.Copies, (b, c) => new { Book = b, Copy = c });

        return loans.Join(
            copyToBook,
            loan => loan.BookCopyId,
            pair => pair.Copy.Id,
            (loan, pair) => new BookLoanLine(
                loan.Id,
                pair.Book.Title,
                loan.BookCopyId,
                loan.ReaderId,
                loan.IssuedOn,
                loan.Status.ToString()))
            .ToList();
    }

    /// <summary>
    /// Бенчмарк швидкодії фільтрації на 100 000 записах (Додаткове завдання 4).
    /// </summary>
    public static (long WithoutIntermediateMs, long WithIntermediateMs) BenchmarkFiltering(int recordsCount = 100_000)
    {
        var testData = Enumerable.Range(1, recordsCount)
            .Select(i => new { Id = i, Sku = $"SKU-{i:D6}", Quantity = i % 50, Unit = i % 2 == 0 ? "прим." : "том" })
            .ToList();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int r = 0; r < 5; r++)
        {
            _ = testData
                .Where(x => x.Quantity < 10)
                .OrderBy(x => x.Quantity)
                .ThenBy(x => x.Sku, StringComparer.Ordinal)
                .Take(10)
                .ToList();
        }
        sw.Stop();
        long timeWithout = sw.ElapsedMilliseconds;

        sw.Restart();
        for (int r = 0; r < 5; r++)
        {
            _ = testData
                .Where(x => x.Quantity < 10)
                .ToList()
                .OrderBy(x => x.Quantity)
                .ThenBy(x => x.Sku, StringComparer.Ordinal)
                .Take(10)
                .ToList();
        }
        sw.Stop();
        long timeWith = sw.ElapsedMilliseconds;

        return (timeWithout, timeWith);
    }
}

