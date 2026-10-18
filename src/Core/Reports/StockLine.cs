namespace Core.Reports;

/// <summary>
/// DTO рядок звіту по залишках книг / позицій каталогу.
/// </summary>
public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);

