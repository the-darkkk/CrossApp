namespace Core.Dto;

public record BookDto(
    string Id,
    string Isbn,
    string Title,
    int Year,
    string? Author = null,
    IReadOnlyList<BookCopyDto>? Copies = null);