using Core.Abstractions;

namespace Cli;

/// <summary>
/// Реалізація приймача сповіщень для виведення в консоль (Додаткове завдання 1).
/// </summary>
public sealed class ConsoleSink : INotificationSink
{
    public void Emit(string message) =>
        Console.WriteLine($"[ConsoleSink] {message}");
}

/// <summary>
/// Реалізація приймача сповіщень для запису у файл (Додаткове завдання 1).
/// </summary>
public sealed class FileSink(string filePath) : INotificationSink
{
    private readonly string _filePath = filePath;

    public void Emit(string message)
    {
        string? dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.AppendAllText(_filePath, $"[FileSink] {message}{Environment.NewLine}");
    }
}

