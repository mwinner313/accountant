using FluentValidation;

namespace Bazaar.Handlers.Commands.ProductProperties.Create;

public class CreateProductPropertyCommandValidator : AbstractValidator<CreateProductPropertyCommand>
{
    public CreateProductPropertyCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
