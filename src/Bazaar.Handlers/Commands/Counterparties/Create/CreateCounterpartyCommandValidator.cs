using FluentValidation;

namespace Bazaar.Handlers.Commands.Counterparties.Create;

public class CreateCounterpartyCommandValidator : AbstractValidator<CreateCounterpartyCommand>
{
    public CreateCounterpartyCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(300);
        RuleForEach(x => x.Phones).ChildRules(p =>
        {
            p.RuleFor(i => i.Number).NotEmpty().MaximumLength(40);
        });
        RuleForEach(x => x.BankAccounts).ChildRules(b =>
        {
            b.RuleFor(i => i.Name).NotEmpty().MaximumLength(200);
            b.RuleFor(i => i.AccountNumber).MaximumLength(50);
            b.RuleFor(i => i.ShebaNumber).MaximumLength(34);
            b.RuleFor(i => i.CardNumber).MaximumLength(20);
        });
    }
}
