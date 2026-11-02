namespace Core.Configuration;

/// <summary>
/// Кореневі параметри конфігурації застосунку.
/// </summary>
public sealed class AppOptions
{
    public StorageOptions Storage { get; init; } = new();
    public LoggingOptions Logging { get; init; } = new();
    public LibraryOptions Library { get; init; } = new();
    public string EnvironmentName { get; init; } = "Development";
}

/// <summary>
/// Параметри зберігання каталогу даних.
/// </summary>
public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "items.json";

    public string ResolveFullPath(string baseDir)
    {
        var root = Path.IsPathRooted(DataPath) ? DataPath : Path.Combine(baseDir, DataPath);
        Directory.CreateDirectory(root);
        return Path.GetFullPath(Path.Combine(root, FileName));
    }
}

/// <summary>
/// Параметри логування.
/// </summary>
public sealed class LoggingOptions
{
    public string MinimumLevel { get; init; } = "Information";
}

/// <summary>
/// Параметри бібліотечного домену (варіант за доменом «Бібліотека»).
/// </summary>
public sealed class LibraryOptions
{
    public int LoanDays { get; init; } = 14;
}

