using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveBookRequestValidator : AbstractValidator<SaveBookRequest>
{
    public SaveBookRequestValidator()
    {
        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("ISBN обов'язковий.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Назва обов'язкова, до 200 символів.")
            .MaximumLength(200).WithMessage("Назва обов'язкова, до 200 символів.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1450, DateTime.UtcNow.Year)
            .WithMessage($"Рік видання має бути в межах від 1450 до {DateTime.UtcNow.Year}.");

        RuleFor(x => x.Copies)
            .GreaterThanOrEqualTo(0).WithMessage("Кількість примірників не може бути від'ємною.");
    }
}

