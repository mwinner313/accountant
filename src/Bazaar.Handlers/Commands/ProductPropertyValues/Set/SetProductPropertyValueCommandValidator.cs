using FluentValidation;

namespace Bazaar.Handlers.Commands.ProductPropertyValues.Set;

public class SetProductPropertyValueCommandValidator : AbstractValidator<SetProductPropertyValueCommand>
{
    public SetProductPropertyValueCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductPropertyId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty().MaximumLength(500);
    }
}
