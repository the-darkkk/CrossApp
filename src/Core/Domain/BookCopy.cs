using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Фізичний примірник книги в бібліотечному фонді.
/// </summary>
public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public bool IsIssued { get; private set; }

    private BookCopy(string id, string isbn, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        IsIssued = isIssued;
    }

    /// <summary>
    /// Фабричний метод для створення примірника книги з перевіркою інваріантів.
    /// </summary>
    public static BookCopy Create(string id, string isbn, bool isIssued = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN примірника не може бути порожнім", nameof(isbn));

        return new BookCopy(id.Trim(), isbn.Trim().ToUpperInvariant(), isIssued);
    }

    /// <summary>
    /// Фіксація видачі примірника читачу.
    /// </summary>
    public void Issue()
    {
        if (IsIssued)
        {
            throw new InvalidOperationException(
                $"Примірник {Id} (ISBN: {Isbn}) вже виданий, повторна видача неможлива");
        }

        IsIssued = true;
    }

    /// <summary>
    /// Фіксація повернення примірника до фонду.
    /// </summary>
    public void Return()
    {
        if (!IsIssued)
        {
            throw new InvalidOperationException(
                $"Примірник {Id} (ISBN: {Isbn}) не був виданий, повернення неможливе");
        }

        IsIssued = false;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, IsIssued);

    public static BookCopy FromDto(BookCopyDto dto) =>
        Create(dto.Id, dto.Isbn, dto.IsIssued);

    public override string ToString() =>
        $"Примірник [{Id}] (ISBN: {Isbn}) — {(IsIssued ? "Виданий" : "В наявності")}";
}
