namespace Core.Dto;

public sealed record MixedImportResult(
    IReadOnlyList<BookDto> Books,
    IReadOnlyList<ReaderDto> Readers,
    IReadOnlyList<string> Errors)
{
    public int Accepted => Books.Count + Readers.Count;
    public int Total => Accepted + Errors.Count;
    public double ErrorPercentage => Total == 0 ? 0.0 : (double)Errors.Count / Total * 100.0;

    public string FormatStats() =>
        $"Усього: {Total} | Прийнято: {Accepted} (книг: {Books.Count}, читачів: {Readers.Count}) | Пропущено: {Errors.Count} | Помилок: {ErrorPercentage:F1}%";
}