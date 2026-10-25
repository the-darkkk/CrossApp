namespace Core;

/// <summary>
/// Виняток порушення доменних правил та інваріантів бізнес-моделі.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

