using Bazaar.Data;
using Extensions.Http.Mvc;
using Infra;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Counterparties.GetCounterpartyById;

public class GetCounterpartyByIdQueryHandler : IQueryHandler<GetCounterpartyByIdQuery, CounterpartyDetailModel>
{
    private readonly BazaarDbContext _dbContext;
    private readonly IExecutionContext _executionContext;

    public GetCounterpartyByIdQueryHandler(BazaarDbContext dbContext, IExecutionContext executionContext)
    {
        _dbContext = dbContext;
        _executionContext = executionContext;
    }

    public async Task<CounterpartyDetailModel> HandleAsync(
        GetCounterpartyByIdQuery parameters,
        CancellationToken cts = default)
    {
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);
        var c = await _dbContext.Counterparties
            .AsNoTracking()
            .Include(x => x.Phones)
            .Include(x => x.BankAccounts)
            .FirstAsync(
                x => x.Id == parameters.CounterpartyId && x.OwnerId == ownerId && !x.Deleted,
                cts);

        return new CounterpartyDetailModel
        {
            CounterpartyId = c.Id,
            FullName = c.FullName,
            Phones = c.Phones.Select(p => new CounterpartyPhoneModel { Number = p.Number }).ToList(),
            BankAccounts = c.BankAccounts
                .Select(b => new CounterpartyBankAccountModel
                {
                    Name = b.Name,
                    AccountNumber = b.AccountNumber,
                    ShebaNumber = b.ShebaNumber,
                    CardNumber = b.CardNumber
                })
                .ToList()
        };
    }
}
