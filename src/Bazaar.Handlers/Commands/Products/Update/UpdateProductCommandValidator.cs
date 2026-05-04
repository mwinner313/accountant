using FluentValidation;

namespace Bazaar.Handlers.Commands.Products.Update;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SellPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BuyPrice).GreaterThanOrEqualTo(0);
    }
}
