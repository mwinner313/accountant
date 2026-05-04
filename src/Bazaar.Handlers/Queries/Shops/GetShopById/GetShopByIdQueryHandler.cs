using Bazaar.Data;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Shops.GetShopById;

public class GetShopByIdQueryHandler : IQueryHandler<GetShopByIdQuery, ShopDetailModel>
{
    private readonly BazaarDbContext _dbContext;

    public GetShopByIdQueryHandler(BazaarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShopDetailModel> HandleAsync(GetShopByIdQuery parameters, CancellationToken cts = default)
    {
        return await _dbContext.Shops
            .AsNoTracking()
            .Where(s => s.Id == parameters.ShopId && !s.Deleted)
            .Select(s => new ShopDetailModel
            {
                ShopId = s.Id,
                Title = s.Title,
                OwnerId = s.OwnerId,
                CreatedOn = s.CreateDate
            })
            .FirstAsync(cts);
    }
}
