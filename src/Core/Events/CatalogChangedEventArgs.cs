namespace Core.Events;

/// <summary>
/// Дані події про зміну стану каталогу або примірників книг.
/// </summary>
public sealed class CatalogChangedEventArgs(
    string id,
    CatalogChangeKind kind,
    int delta,
    int quantityAfter,
    string? copyId = null,
    string? readerId = null,
    DateTimeOffset? dueDate = null) : EventArgs
{
    public string Id { get; } = id;
    public CatalogChangeKind Kind { get; } = kind;
    public int Delta { get; } = delta;
    public int QuantityAfter { get; } = quantityAfter;
    public string? CopyId { get; } = copyId;
    public string? ReaderId { get; } = readerId;
    public DateTimeOffset? DueDate { get; } = dueDate;
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}

