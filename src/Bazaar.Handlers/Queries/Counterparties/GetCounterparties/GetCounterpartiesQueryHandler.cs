using Bazaar.Data;
using Extensions.Http.Mvc;
using Extensions.Sliding;
using Infra;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Counterparties.GetCounterparties;

public class GetCounterpartiesQueryHandler : IQueryHandler<GetCounterpartiesQuery, SlidingCollectionWrapper<CounterpartyListModel>>
{
    private readonly BazaarDbContext _dbContext;
    private readonly IExecutionContext _executionContext;

    public GetCounterpartiesQueryHandler(BazaarDbContext dbContext, IExecutionContext executionContext)
    {
        _dbContext = dbContext;
        _executionContext = executionContext;
    }

    public Task<SlidingCollectionWrapper<CounterpartyListModel>> HandleAsync(
        GetCounterpartiesQuery parameters,
        CancellationToken cts = default)
    {
        parameters.Pagination ??= new SlidingParams { Skip = 0, Take = 20 };
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);

        var query = _dbContext.Counterparties
            .AsNoTracking()
            .Where(c => c.OwnerId == ownerId && !c.Deleted);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var s = parameters.Search;
            query = query.Where(c =>
                c.FullName.Contains(s)
                || c.Phones.Any(p => p.Number.Contains(s))
                || c.BankAccounts.Any(b =>
                    b.Name.Contains(s)
                    || b.AccountNumber.Contains(s)
                    || b.ShebaNumber.Contains(s)
                    || b.CardNumber.Contains(s)));
        }

        return query
            .OrderByDescending(c => c.CreateDate)
            .Select(c => new CounterpartyListModel
            {
                CounterpartyId = c.Id,
                FullName = c.FullName
            })
            .ToSlidingCollectionWrapperAsync(parameters.Pagination);
    }
}
