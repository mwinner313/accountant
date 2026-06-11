using Bazaar.Core.Entities.Counterparty;
using Bazaar.Core.Entities.Counterparty.Events;
using Extensions.Http.Mvc;
using Infra;
using Infra.Commands;
using Infra.EFCore;

namespace Bazaar.Handlers.Commands.Counterparties.Create;

public class CreateCounterpartyCommandHandler : ICommandHandler<CreateCounterpartyCommand, CreateCounterpartyCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;

    public CreateCounterpartyCommandHandler(IUnitOfWork unitOfWork, IExecutionContext executionContext)
    {
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
    }

    public async Task<CreateCounterpartyCommandResult> HandleAsync(
        CreateCounterpartyCommand command,
        CancellationToken cancellationToken)
    {
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);
        var phones = command.Phones
            .Select(p => new CounterpartyPhoneData(p.Number))
            .ToList();
        var banks = command.BankAccounts
            .Select(b => new CounterpartyBankAccountData(b.Name, b.AccountNumber, b.ShebaNumber, b.CardNumber))
            .ToList();

        var counterparty = new Counterparty(ownerId, command.FullName, phones, banks);
        _unitOfWork.Repo<Counterparty>().Add(counterparty);
        await _unitOfWork.Save(counterparty);

        return new CreateCounterpartyCommandResult { CounterpartyId = counterparty.Id };
    }
}
