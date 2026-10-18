namespace Core.Reports;

/// <summary>
/// DTO детального рядка видачі з інформацією про книгу (Join для додаткового завдання 1).
/// </summary>
public sealed record BookLoanLine(string LoanId, string BookTitle, string CopyId, string ReaderId, DateTime IssuedOn, string Status);

