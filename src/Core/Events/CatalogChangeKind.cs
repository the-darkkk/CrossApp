namespace Core.Events;

/// <summary>
/// Типи подій зміни стану каталогу бібліотеки.
/// </summary>
public enum CatalogChangeKind
{
    BookAdded,
    CopyIssued,
    CopyReturned,
    Added,
    Received,
    Issued,
    Removed
}
