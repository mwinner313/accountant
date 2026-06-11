using Bazaar.Data;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Factors.GetFactorById;

public class GetFactorByIdQueryHandler : IQueryHandler<GetFactorByIdQuery, FactorDetailModel>
{
    private readonly BazaarDbContext _dbContext;

    public GetFactorByIdQueryHandler(BazaarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FactorDetailModel> HandleAsync(GetFactorByIdQuery parameters, CancellationToken cts = default)
    {
        var factor = await _dbContext.Factors
            .AsNoTracking()
            .Include(f => f.Items)
            .Where(f => f.Id == parameters.FactorId && !f.Deleted)
            .FirstAsync(cts);

        var productIds = factor.Items.Select(i => i.ProductId).ToList();
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(cts);

        string? counterpartyFullName = null;
        if (factor.CounterpartyId.HasValue)
        {
            counterpartyFullName = await _dbContext.Counterparties
                .AsNoTracking()
                .Where(c => c.Id == factor.CounterpartyId)
                .Select(c => c.FullName)
                .FirstOrDefaultAsync(cts);
        }

        return new FactorDetailModel
        {
            FactorId = factor.Id,
            ShopId = factor.ShopId,
            Type = factor.Type,
            CounterpartyId = factor.CounterpartyId,
            CounterpartyFullName = counterpartyFullName,
            Notes = factor.Notes,
            Date = factor.Date,
            IsReversed = factor.IsReversed,
            CreatedOn = factor.CreateDate,
            Items = factor.Items.Select(i => new FactorItemDetailModel
            {
                ProductId = i.ProductId,
                ProductName = products.FirstOrDefault(p => p.Id == i.ProductId)?.Name ?? "Unknown",
                Amount = i.Amount,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
