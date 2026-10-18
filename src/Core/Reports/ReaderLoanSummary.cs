namespace Core.Reports;

/// <summary>
/// DTO зведеного звіту за читачами та їхніми видачами (GroupJoin для додаткового завдання 1).
/// </summary>
public sealed record ReaderLoanSummary(string ReaderId, string ReaderName, int TotalLoans, int ActiveLoans);

