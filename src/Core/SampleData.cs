using Core.Domain;

namespace Core;

/// <summary>
/// Початкові дані для демонстрації та аналітичних звітів (20 записів з різними одиницями та залишками).
/// </summary>
public static class SampleData
{
    /// <summary>
    /// Повертає список 20 книг із різними одиницями виміру, жанрами та кількістю примірників.
    /// </summary>
    public static List<Book> Books()
    {
        return
        [
            CreateWithCopies("B-001", "978-0-13-235088-4", "Clean Code", 2008, "Robert C. Martin", "прим.", "Програмування", 25),
            CreateWithCopies("B-002", "978-0-201-61622-4", "The Pragmatic Programmer", 1999, "Andrew Hunt", "прим.", "Програмування", 15),
            CreateWithCopies("B-003", "978-0-13-597444-5", "Refactoring", 2018, "Martin Fowler", "прим.", "Архітектура", 20),
            CreateWithCopies("B-004", "978-0-262-03384-8", "Introduction to Algorithms", 2009, "Thomas Cormen", "том", "Алгоритми", 30),
            CreateWithCopies("B-005", "978-0-596-51774-8", "JavaScript: The Good Parts", 2008, "Douglas Crockford", "прим.", "Програмування", 12),
            CreateWithCopies("B-006", "978-0-321-12521-7", "Domain-Driven Design", 2003, "Eric Evans", "том", "Архітектура", 18),
            CreateWithCopies("B-007", "978-0-13-449416-6", "The Go Programming Language", 2015, "Alan Donovan", "прим.", "Програмування", 14),
            CreateWithCopies("B-008", "978-0-13-110362-7", "The C Programming Language", 1988, "Brian Kernighan", "прим.", "Класика", 2),
            CreateWithCopies("B-009", "978-0-321-14653-3", "Test-Driven Development", 2002, "Kent Beck", "прим.", "Архітектура", 16),
            CreateWithCopies("B-010", "978-0-13-468599-1", "The Clean Coder", 2011, "Robert C. Martin", "прим.", "Програмування", 22),
            CreateWithCopies("B-011", "978-0-59-651798-4", "Head First Design Patterns", 2004, "Eric Freeman", "том", "Архітектура", 28),
            CreateWithCopies("B-012", "978-0-13-476904-3", "Clean Architecture", 2017, "Robert C. Martin", "прим.", "Архітектура", 35),
            CreateWithCopies("B-013", "978-1-49-195016-0", "Programming Rust", 2021, "Jim Blandy", "прим.", "Програмування", 19),
            CreateWithCopies("B-014", "978-0-13-468474-1", "Effective Java", 2017, "Joshua Bloch", "прим.", "Програмування", 17),
            CreateWithCopies("B-015", "978-0-59-651798-4", "C# in Depth", 2019, "Jon Skeet", "прим.", "Програмування", 24),
            CreateWithCopies("B-016", "978-1-61729-453-2", "ASP.NET Core in Action", 2021, "Andrew Lock", "ел.вид.", "Програмування", 45),
            CreateWithCopies("B-017", "978-0-201-48567-7", "Design Patterns", 1994, "Gang of Four", "том", "Класика", 3),
            CreateWithCopies("B-018", "978-1-49-207799-2", "Concurrency in C# Cookbook", 2019, "Stephen Cleary", "ел.вид.", "Програмування", 40),
            CreateWithCopies("B-019", "978-0-13-277804-6", "Computer Networks", 2010, "Andrew Tanenbaum", "том", "Класика", 26),
            CreateWithCopies("B-020", "978-0-00-111222-3", "Software Engineering Digest", 2024, "IEEE Review", "журнал", "Періодика", 5),
        ];
    }

    /// <summary>
    /// Повертає список читачів для демонстрації.
    /// </summary>
    public static List<Reader> Readers() =>
    [
        Reader.Create("R-001", "Олег Гаргас",     "+380501112233", "oleh@example.com"),
        Reader.Create("R-002", "Іван Франко",      "+380672223344"),
        Reader.Create("R-003", "Тарас Шевченко",   "+380933334455"),
        Reader.Create("R-004", "Леся Українка",    "+380994445566", "lesya@example.com"),
        Reader.Create("R-005", "Марко Вовчок",     "+380501234567"),
    ];

    /// <summary>
    /// Повертає тестові видачі для звітів Join / GroupJoin (Додаткове завдання 1).
    /// </summary>
    public static List<Loan> Loans(IEnumerable<Book>? books = null)
    {
        var bookList = (books ?? Books()).ToList();
        var loans = new List<Loan>();

        void TryAddLoan(string loanId, string bookId, string readerId, int daysAgo, bool isClosed)
        {
            var book = bookList.FirstOrDefault(b => b.Id == bookId);
            if (book == null || book.Copies.Count == 0) return;
            var copy = book.Copies.FirstOrDefault(c => !c.IsIssued) ?? book.Copies[0];
            var issueDate = DateTime.Now.AddDays(-daysAgo);
            if (isClosed)
            {
                var l = Loan.Create(loanId, copy.Id, readerId, issueDate, issueDate.AddDays(10), LoanStatus.Closed);
                loans.Add(l);
            }
            else
            {
                var l = Loan.Open(loanId, copy, readerId, issueDate);
                loans.Add(l);
            }
        }

        TryAddLoan("L-001", "B-001", "R-001", 12, false);
        TryAddLoan("L-002", "B-002", "R-001", 30, true);
        TryAddLoan("L-003", "B-003", "R-002", 5, false);
        TryAddLoan("L-004", "B-004", "R-002", 40, true);
        TryAddLoan("L-005", "B-006", "R-002", 20, true);
        TryAddLoan("L-006", "B-007", "R-003", 2, false);
        TryAddLoan("L-007", "B-010", "R-004", 15, false);
        TryAddLoan("L-008", "B-012", "R-004", 45, true);

        return loans;
    }

    private static Book CreateWithCopies(
        string id,
        string isbn,
        string title,
        int year,
        string author,
        string unit,
        string genre,
        int copiesCount)
    {
        var book = Book.Create(id, isbn, title, year, author, unit, genre);
        for (int i = 1; i <= copiesCount; i++)
        {
            book.AddCopy($"BC-{id}-{i:D2}");
        }
        return book;
    }
}
