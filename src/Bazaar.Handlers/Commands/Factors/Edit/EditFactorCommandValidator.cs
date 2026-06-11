using FluentValidation;

namespace Bazaar.Handlers.Commands.Factors.Edit;

public class EditFactorCommandValidator : AbstractValidator<EditFactorCommand>
{
    public EditFactorCommandValidator()
    {
        RuleFor(x => x.FactorId).NotEmpty();
        RuleFor(x => x.CounterpartyId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Factor must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Amount).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
