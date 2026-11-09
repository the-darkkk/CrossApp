namespace Api.Contracts;

public sealed record BookDto(string Id, string Isbn, string Title, int Year, string? Author, int Copies, string Unit = "прим.");

