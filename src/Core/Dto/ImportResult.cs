namespace Core.Dto;

public sealed record ImportResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<string> Errors)
{
    public int Total => Items.Count + Errors.Count;
    public double ErrorPercentage => Total == 0 ? 0.0 : (double)Errors.Count / Total * 100.0;

    public string FormatStats() =>
        $"Усього: {Total} | Прийнято: {Items.Count} | Пропущено: {Errors.Count} | Помилок: {ErrorPercentage:F1}%";
}