using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Сутність процесу видачі примірника книги читачеві.
/// </summary>
public sealed class Loan
{
    public const int MaxActiveLoansPerReader = 5;

    public string Id { get; }
    public string BookCopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }
    public LoanStatus Status { get; private set; }

    private Loan(
        string id,
        string bookCopyId,
        string readerId,
        DateTime issuedOn,
        DateTime? returnedOn,
        LoanStatus status)
    {
        Id = id;
        BookCopyId = bookCopyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        ReturnedOn = returnedOn;
        Status = status;
    }

    /// <summary>
    /// Фабричний метод для оформлення нової видачі.
    /// Перевіряє як стан примірника, так і ліміт відкритих видач читача (інваріант двох сутностей).
    /// </summary>
    public static Loan Open(
        string id,
        BookCopy copy,
        string readerId,
        DateTime issuedOn,
        int activeReaderLoansCount = 0)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));

        ArgumentNullException.ThrowIfNull(copy, nameof(copy));

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        if (issuedOn == default)
            throw new ArgumentException("Дата видачі має бути вказана", nameof(issuedOn));

        // Додаткове завдання 2: Інваріант двох сутностей (ліміт видач читача)
        if (activeReaderLoansCount >= MaxActiveLoansPerReader)
        {
            throw new InvalidOperationException(
                $"Неможливо відкрити видачу {id.Trim()} для читача {readerId.Trim()}: " +
                $"перевищено ліміт відкритих видач (максимум {MaxActiveLoansPerReader}, наразі {activeReaderLoansCount})");
        }

        // Перевірка інваріанту стану примірника: видача змінює стан примірника
        copy.Issue();

        return new Loan(
            id.Trim(),
            copy.Id,
            readerId.Trim(),
            issuedOn,
            null,
            LoanStatus.Active);
    }

    /// <summary>
    /// Фабричний метод для відновлення сутності (наприклад, з DTO/сховища) з повною перевіркою інваріантів.
    /// </summary>
    public static Loan Create(
        string id,
        string bookCopyId,
        string readerId,
        DateTime issuedOn,
        DateTime? returnedOn,
        LoanStatus status)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(bookCopyId))
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(bookCopyId));

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        if (issuedOn == default)
            throw new ArgumentException("Дата видачі має бути вказана", nameof(issuedOn));

        if (status == LoanStatus.Closed)
        {
            if (returnedOn is null)
            {
                throw new ArgumentException("Для закритої видачі дата повернення є обов'язковою", nameof(returnedOn));
            }

            if (returnedOn < issuedOn)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(returnedOn),
                    returnedOn,
                    $"Дата повернення ({returnedOn:yyyy-MM-dd}) не може бути раніше дати видачі ({issuedOn:yyyy-MM-dd})");
            }
        }
        else if (status == LoanStatus.Active && returnedOn is not null)
        {
            throw new ArgumentException("Активна видача не повинна містити дату повернення", nameof(returnedOn));
        }
        else if (status == LoanStatus.Cancelled && returnedOn is not null && returnedOn < issuedOn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(returnedOn),
                returnedOn,
                $"Дата скасування/повернення ({returnedOn:yyyy-MM-dd}) не може бути раніше дати видачі ({issuedOn:yyyy-MM-dd})");
        }

        return new Loan(
            id.Trim(),
            bookCopyId.Trim(),
            readerId.Trim(),
            issuedOn,
            returnedOn,
            status);
    }

    /// <summary>
    /// Успішне закриття видачі (повернення книги).
    /// Перевіряє допустимість переходу стану за допомогою switch expression (Додаткове завдання 3).
    /// </summary>
    public void Close(DateTime returnedOn, BookCopy? copy = null)
    {
        if (returnedOn < IssuedOn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(returnedOn),
                returnedOn,
                $"Дата повернення ({returnedOn:yyyy-MM-dd}) не може бути раніше дати видачі ({IssuedOn:yyyy-MM-dd})");
        }

        TransitionTo(LoanStatus.Closed);
        ReturnedOn = returnedOn;
        copy?.Return();
    }

    /// <summary>
    /// Скасування видачі (помилкова реєстрація або анулювання).
    /// </summary>
    public void Cancel(string reason, BookCopy? copy = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Причина скасування видачі обов'язкова", nameof(reason));

        TransitionTo(LoanStatus.Cancelled);
        copy?.Return();
    }

    /// <summary>
    /// Додаткове завдання 3: Перевірка допустимих переходів станів через switch expression.
    /// </summary>
    private void TransitionTo(LoanStatus newStatus)
    {
        bool isAllowed = (Status, newStatus) switch
        {
            (LoanStatus.Active, LoanStatus.Closed) => true,
            (LoanStatus.Active, LoanStatus.Cancelled) => true,
            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                $"Неприпустимий перехід стану видачі {Id}: неможливо перевести з {Status} у {newStatus}");
        }

        Status = newStatus;
    }

    public LoanDto ToDto() => new(Id, BookCopyId, ReaderId, IssuedOn, ReturnedOn, Status);

    public static Loan FromDto(LoanDto dto) =>
        Create(dto.Id, dto.BookCopyId, dto.ReaderId, dto.IssuedOn, dto.ReturnedOn, dto.Status);

    public override string ToString() =>
        $"Видача [{Id}]: примірник {BookCopyId} -> читач {ReaderId} | " +
        $"Видано: {IssuedOn:yyyy-MM-dd}" +
        (ReturnedOn.HasValue ? $" | Повернуто: {ReturnedOn.Value:yyyy-MM-dd}" : "") +
        $" | Стан: {Status}";
}
