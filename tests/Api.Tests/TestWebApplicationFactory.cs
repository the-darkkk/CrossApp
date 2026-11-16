using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"crossapp_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:AppDb", $"Data Source={_dbPath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            // Видалення тимчасової тестової БД та супутніх WAL файлів
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
            var shm = $"{_dbPath}-shm";
            if (File.Exists(shm)) File.Delete(shm);
            var wal = $"{_dbPath}-wal";
            if (File.Exists(wal)) File.Delete(wal);
        }
        catch
        {
            // Ігноруємо помилки очищення файлів ОС
        }
    }
}
