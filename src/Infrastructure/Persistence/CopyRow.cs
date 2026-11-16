namespace Infrastructure.Persistence;

public sealed class CopyRow
{
    public string Id { get; set; } = "";
    public string BookId { get; set; } = "";
    public string Isbn { get; set; } = "";
    public bool IsIssued { get; set; }

    public BookRow Book { get; set; } = null!;
}
