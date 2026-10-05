using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Додаткове завдання 3: Фабрика для створення сховища за аргументами командного рядка.
/// Порівняння: у Program.cs можна обійтися тернарним оператором, але фабрика
/// інкапсулює логіку вибору реалізації і спрощує composition root.
/// </summary>
public static class StoreFactory
{
    /// <summary>
    /// Створює реалізацію IBookStore за переданими аргументами:
    /// --file → FileBookStore з файлом за вказаним шляхом;
    /// інакше → InMemoryBookStore з початковими даними.
    /// </summary>
    public static IBookStore Create(string[] args, string filePath, IEnumerable<Book>? seed = null)
    {
        bool useFile = args.Contains("--file");

        return useFile
            ? new FileBookStore(filePath)
            : new InMemoryBookStore(seed);
    }
}
