using Core.Domain;

namespace Core;

/// <summary>
/// Початкові дані для демонстрації та тижня 7.
/// 15 книг з предметної області «Бібліотека»; у кожної — по 1-2 примірники.
/// </summary>
public static class SampleData
{
    /// <summary>
    /// Повертає 15 книг-сутностей із зареєстрованими примірниками.
    /// </summary>
    public static List<Book> Books()
    {
        var books = new List<Book>
        {
            Create("B-001", "978-0-13-235088-4", "Clean Code", 2008, "Robert C. Martin",          "BC-001", "BC-002"),
            Create("B-002", "978-0-201-61622-4", "The Pragmatic Programmer", 1999, "Andrew Hunt",  "BC-003"),
            Create("B-003", "978-0-13-597444-5", "Refactoring", 2018, "Martin Fowler",             "BC-004", "BC-005"),
            Create("B-004", "978-0-262-03384-8", "Introduction to Algorithms", 2009, "Thomas Cormen", "BC-006"),
            Create("B-005", "978-0-596-51774-8", "JavaScript: The Good Parts", 2008, "Douglas Crockford", "BC-007"),
            Create("B-006", "978-0-321-12521-7", "Domain-Driven Design", 2003, "Eric Evans",       "BC-008", "BC-009"),
            Create("B-007", "978-0-13-449416-6", "The Go Programming Language", 2015, "Alan Donovan", "BC-010"),
            Create("B-008", "978-0-13-110362-7", "The C Programming Language", 1988, "Brian Kernighan", "BC-011"),
            Create("B-009", "978-0-321-14653-3", "Test-Driven Development", 2002, "Kent Beck",     "BC-012"),
            Create("B-010", "978-0-13-468599-1", "The Clean Coder", 2011, "Robert C. Martin",      "BC-013"),
            Create("B-011", "978-0-59-651798-4", "Head First Design Patterns", 2004, "Eric Freeman", "BC-014", "BC-015"),
            Create("B-012", "978-0-13-476904-3", "Clean Architecture", 2017, "Robert C. Martin",   "BC-016"),
            Create("B-013", "978-1-49-195016-0", "Programming Rust", 2021, "Jim Blandy",           "BC-017"),
            Create("B-014", "978-0-13-468474-1", "Effective Java", 2017, "Joshua Bloch",           "BC-018"),
            Create("B-015", "978-0-59-651798-4", "C# in Depth", 2019, "Jon Skeet",                 "BC-019", "BC-020"),
        };

        return books;
    }

    /// <summary>
    /// Повертає 5 читачів для демонстрації.
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
    /// Створює книгу з примірниками в одному виклику.
    /// </summary>
    private static Book Create(string id, string isbn, string title, int year, string author, params string[] copyIds)
    {
        var book = Book.Create(id, isbn, title, year, author);
        foreach (string copyId in copyIds)
            book.AddCopy(copyId);
        return book;
    }
}
