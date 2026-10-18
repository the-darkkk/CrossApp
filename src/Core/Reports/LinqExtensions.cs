namespace Core.Reports;

/// <summary>
/// Власні методи-розширення LINQ (Додаткове завдання 2).
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// Повертає топ-N елементів за числовим ключем.
    /// </summary>
    public static IReadOnlyList<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, int> keySelector,
        int count)
    {
        return source
            .OrderByDescending(keySelector)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Повертає топ-N елементів із детермінованим порядком (первинний + вторинний ключ).
    /// </summary>
    public static IReadOnlyList<T> TopBy<T, TSecondary>(
        this IEnumerable<T> source,
        Func<T, int> primaryKeySelector,
        Func<T, TSecondary> secondaryKeySelector,
        int count)
    {
        return source
            .OrderByDescending(primaryKeySelector)
            .ThenBy(secondaryKeySelector)
            .Take(count)
            .ToList();
    }
}

