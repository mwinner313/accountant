using Bazaar.Core.Entities.Counterparty;
using Bazaar.Core.Entities.Counterparty.Events;
using Bazaar.Data;
using Extensions.Http.Mvc;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Counterparties.Update;

public class UpdateCounterpartyCommandHandler : ICommandHandler<UpdateCounterpartyCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;
    private readonly IExecutionContext _executionContext;

    public UpdateCounterpartyCommandHandler(
        IUnitOfWork unitOfWork,
        BazaarDbContext dbContext,
        IExecutionContext executionContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _executionContext = executionContext;
    }

    public async Task<Empty> HandleAsync(UpdateCounterpartyCommand command, CancellationToken cancellationToken)
    {
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);
        var counterparty = await _dbContext.Counterparties
            .Include(c => c.Phones)
            .Include(c => c.BankAccounts)
            .FirstAsync(
                c => c.Id == command.CounterpartyId && c.OwnerId == ownerId && !c.Deleted,
                cancellationToken);

        _dbContext.CounterpartyPhones.RemoveRange(counterparty.Phones);
        _dbContext.CounterpartyBankAccounts.RemoveRange(counterparty.BankAccounts);
        counterparty.Phones.Clear();
        counterparty.BankAccounts.Clear();

        var phones = command.Phones.Select(p => new CounterpartyPhoneData(p.Number)).ToList();
        var banks = command.BankAccounts
            .Select(b => new CounterpartyBankAccountData(b.Name, b.AccountNumber, b.ShebaNumber, b.CardNumber))
            .ToList();

        counterparty.Update(command.FullName, phones, banks);
        await _unitOfWork.Save(counterparty);

        return Empty.Instance;
    }
}
