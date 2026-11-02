namespace Core.Dto;

/// <summary>
/// DTO сутності для збереження у форматі JSON (тиждень 9).
/// </summary>
public record ProductDto(
    string Id,
    string Sku,
    string Name,
    string Unit,
    int Quantity);

