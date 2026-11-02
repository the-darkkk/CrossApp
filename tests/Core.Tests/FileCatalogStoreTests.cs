using Core.Domain;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class FileCatalogStoreTests
{
    [Fact]
    public void Store_SavesAndLoadsItems_WithCleanCyrillic()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"store_test_{Guid.NewGuid():N}", "items.json");
        try
        {
            var store = new FileCatalogStore(tempFile);
            var book = Book.Create("B-TEST-1", "978-0-12-345678-9", "Тестова книга", 2024, unit: "прим.");
            book.AddCopy("C-1");
            book.AddCopy("C-2");

            store.Add(book);

            Assert.True(File.Exists(tempFile));
            var rawJson = File.ReadAllText(tempFile);
            Assert.Contains("Тестова книга", rawJson); // перевірка не-екранування кирилиці (\uXXXX)
            Assert.DoesNotContain(@"\u0422", rawJson);

            // новий екземпляр сховища над тим самим файлом
            var reloadedStore = new FileCatalogStore(tempFile);
            var loaded = reloadedStore.GetById("B-TEST-1");

            Assert.NotNull(loaded);
            Assert.Equal("978-0-12-345678-9", loaded.Sku);
            Assert.Equal(2, loaded.Quantity);
        }
        finally
        {
            var dir = Path.GetDirectoryName(tempFile);
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Store_ThrowsDomainException_OnCorruptJson()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"corrupt_store_{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(tempFile, "{ invalid json format");
            var store = new FileCatalogStore(tempFile);

            var ex = Assert.Throws<Core.DomainException>(() => store.List());
            Assert.Contains("Помилка десеріалізації JSON", ex.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}

