using Bazaar.Data;
using Extensions.Sliding;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Factors.GetShopFactors;

public class GetShopFactorsQueryHandler : IQueryHandler<GetShopFactorsQuery, SlidingCollectionWrapper<FactorModel>>
{
    private readonly BazaarDbContext _dbContext;

    public GetShopFactorsQueryHandler(BazaarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SlidingCollectionWrapper<FactorModel>> HandleAsync(GetShopFactorsQuery parameters, CancellationToken cts = default)
    {
        parameters.Pagination ??= new SlidingParams { Skip = 0, Take = 20 };

        var query = _dbContext.Factors
            .AsNoTracking()
            .Where(f => f.ShopId == parameters.ShopId && !f.Deleted);

        if (parameters.Type.HasValue)
            query = query.Where(f => f.Type == parameters.Type);

        return query
            .OrderByDescending(f => f.Date)
            .Select(f => new FactorModel
            {
                FactorId = f.Id,
                CounterpartyId = f.CounterpartyId,
                CounterpartyFullName = f.CounterpartyId == null
                    ? null
                    : _dbContext.Counterparties
                        .Where(c => c.Id == f.CounterpartyId && !c.Deleted)
                        .Select(c => c.FullName)
                        .FirstOrDefault(),
                Type = f.Type,
                Notes = f.Notes,
                Date = f.Date,
                IsReversed = f.IsReversed,
                ItemCount = f.Items.Count,
                CreatedOn = f.CreateDate
            })
            .ToSlidingCollectionWrapperAsync(parameters.Pagination);
    }
}
