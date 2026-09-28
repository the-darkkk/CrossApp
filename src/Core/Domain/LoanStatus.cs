namespace Core.Domain;

/// <summary>
/// Стани життєвого циклу видачі книги.
/// Допустимі переходи:
/// Active -> Closed
/// Active -> Cancelled
/// </summary>
public enum LoanStatus
{
    Active,
    Closed,
    Cancelled
}
