namespace Contracts;

public sealed record SaveBookRequest(
    string Isbn,
    string Title,
    int Year,
    string? Author = null,
    int Copies = 1,
    string Unit = "прим.",
    string? Genre = null)
{
    public bool TryValidate(out Dictionary<string, string[]> errors)
    {
        var found = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(Isbn)) found[nameof(Isbn)] = ["ISBN обов'язковий."];
        if (string.IsNullOrWhiteSpace(Title) || Title.Length > 200)
            found[nameof(Title)] = ["Назва обов'язкова, до 200 символів."];
        if (Year < 1450 || Year > DateTime.UtcNow.Year)
            found[nameof(Year)] = [$"Рік видання має бути в межах від 1450 до {DateTime.UtcNow.Year}."];
        if (Copies < 0) found[nameof(Copies)] = ["Кількість примірників не може бути від'ємною."];
        errors = found;
        return found.Count == 0;
    }
}
