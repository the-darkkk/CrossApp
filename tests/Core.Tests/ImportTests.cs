using Core.Domain;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class ImportTests
{
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new CatalogService(new InMemoryBookStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    [Fact]
    public async Task ImportAsync_WithCorruptData_ThrowsDomainException_AndCleansUpFile()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"corrupt_test_{Guid.NewGuid():N}.json");
        try
        {
            // Некоректний запис: рік 1111 порушує інваріант [1450..поточний рік]
            await File.WriteAllTextAsync(tempFile, """
            [
                { "id": "B-ERR", "isbn": "978-0-00-000000-0", "title": "Помилкова книга", "year": 1111 }
            ]
            """);

            var service = new CatalogService(new InMemoryBookStore());

            var ex = await Assert.ThrowsAsync<DomainException>(
                () => service.ImportAsync(new JsonCatalogImporter(), tempFile));

            Assert.Contains("Запис 1", ex.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportAsync_WithLinkedToken_ThrowsOnTimeout()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        await Task.Delay(40); // чекаємо спрацювання таймауту

        var service = new CatalogService(new InMemoryBookStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", linkedCts.Token));

        Assert.False(userCts.IsCancellationRequested);
        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.True(linkedCts.IsCancellationRequested);
    }

    [Fact]
    public async Task ImportStreamAsync_ImportsItemsSuccessfully()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"stream_test_{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, """
            [
                { "id": "S-1", "isbn": "978-0-11-111111-1", "title": "Потокова книга 1", "year": 2020 },
                { "id": "S-2", "isbn": "978-0-22-222222-2", "title": "Потокова книга 2", "year": 2021 }
            ]
            """);

            var service = new CatalogService(new InMemoryBookStore());
            var count = await service.ImportStreamAsync(new JsonCatalogImporter(), tempFile);

            Assert.Equal(2, count);
            Assert.NotNull(service.Find("S-1"));
            Assert.NotNull(service.Find("S-2"));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportAsync_ReportsProgress()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"progress_test_{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, """
            [
                { "id": "P-1", "isbn": "978-0-11-111111-1", "title": "Прогрес 1", "year": 2020 },
                { "id": "P-2", "isbn": "978-0-22-222222-2", "title": "Прогрес 2", "year": 2021 }
            ]
            """);

            var progressList = new List<int>();
            var progress = new SyncProgress<int>(progressList.Add);

            var service = new CatalogService(new InMemoryBookStore());
            var count = await service.ImportAsync(new JsonCatalogImporter(), tempFile, progress);

            Assert.Equal(2, count);
            Assert.Contains(100, progressList);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private sealed class SyncProgress<T>(Action<T> handler) : IProgress<T>
    {
        public void Report(T value) => handler(value);
    }
}

