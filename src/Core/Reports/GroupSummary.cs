namespace Core.Reports;

/// <summary>
/// DTO підсумку групи за категорією або одиницею виміру.
/// </summary>
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);

