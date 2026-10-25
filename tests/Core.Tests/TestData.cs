using Core.Domain;

namespace Core.Tests;

/// <summary>
/// Фіксований набір тестових даних для ізольованого тестування Core.
/// </summary>
public static class TestData
{
    public static List<Book> Items()
    {
        var b1 = Book.Create("1", "SKU-BIG", "Велика книга", 2020, unit: "шт");
        for (int i = 0; i < 20; i++) b1.AddCopy($"C1-{i}");

        var b2 = Book.Create("2", "SKU-MID", "Середня книга", 2021, unit: "шт");
        for (int i = 0; i < 15; i++) b2.AddCopy($"C2-{i}");

        var b3 = Book.Create("3", "SKU-LOW1", "Мала книга 1", 2022, unit: "м");
        b3.AddCopy("C3-1");
        b3.AddCopy("C3-2");

        var b4 = Book.Create("4", "SKU-LOW2", "Мала книга 2", 2023, unit: "м");
        b4.AddCopy("C4-1");

        return [b1, b2, b3, b4];
    }
}

