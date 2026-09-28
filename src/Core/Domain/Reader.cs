using Core.Dto;

namespace Core.Domain;

/// <summary>
/// Сутність читача бібліотеки.
/// </summary>
public sealed class Reader
{
    public string Id { get; }
    public string FullName { get; }
    public string Phone { get; }
    public string? Email { get; }

    private Reader(string id, string fullName, string phone, string? email)
    {
        Id = id;
        FullName = fullName;
        Phone = phone;
        Email = email;
    }

    /// <summary>
    /// Фабричний метод створення читача з валідацією полів.
    /// </summary>
    public static Reader Create(string id, string fullName, string phone, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("ПІБ читача не може бути порожнім", nameof(fullName));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Номер телефону читача обов'язковий", nameof(phone));

        return new Reader(
            id.Trim(),
            fullName.Trim(),
            phone.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim());
    }

    public ReaderDto ToDto() => new(Id, FullName, Phone, Email);

    public static Reader FromDto(ReaderDto dto) =>
        Create(dto.Id, dto.FullName, dto.Phone, dto.Email);

    public override string ToString() =>
        $"Читач [{Id}] {FullName} (тел: {Phone})" + (Email is not null ? $" <{Email}>" : "");
}
