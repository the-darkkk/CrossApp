namespace Infrastructure.Persistence;

public sealed class BookRow
{
    public string Id { get; set; } = "";
    public string Isbn { get; set; } = "";
    public string Title { get; set; } = "";
    public int Year { get; set; }
    public string? Author { get; set; }
    public string Unit { get; set; } = "прим.";
    public string? Genre { get; set; }

    public List<CopyRow> Copies { get; set; } = [];
}
