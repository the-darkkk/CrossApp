namespace Core.Abstractions;

/// <summary>
/// Інтерфейс приймача сповіщень (для альтернативи подіям через DI).
/// </summary>
public interface INotificationSink
{
    void Emit(string message);
}

