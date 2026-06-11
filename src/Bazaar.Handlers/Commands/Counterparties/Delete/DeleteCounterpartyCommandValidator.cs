using FluentValidation;

namespace Bazaar.Handlers.Commands.Counterparties.Delete;

public class DeleteCounterpartyCommandValidator : AbstractValidator<DeleteCounterpartyCommand>
{
    public DeleteCounterpartyCommandValidator()
    {
        RuleFor(x => x.CounterpartyId).NotEmpty();
    }
}
