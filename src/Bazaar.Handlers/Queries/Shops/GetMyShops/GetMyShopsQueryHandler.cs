using Bazaar.Data;
using Extensions.Http.Mvc;
using Extensions.Sliding;
using Infra;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Shops.GetMyShops;

public class GetMyShopsQueryHandler : IQueryHandler<GetMyShopsQuery, SlidingCollectionWrapper<ShopModel>>
{
    private readonly BazaarDbContext _dbContext;
    private readonly IExecutionContext _executionContext;

    public GetMyShopsQueryHandler(BazaarDbContext dbContext, IExecutionContext executionContext)
    {
        _dbContext = dbContext;
        _executionContext = executionContext;
    }

    public Task<SlidingCollectionWrapper<ShopModel>> HandleAsync(GetMyShopsQuery parameters, CancellationToken cts = default)
    {
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);

        parameters.Pagination ??= new SlidingParams { Skip = 0, Take = 20 };

        return _dbContext.Shops
            .AsNoTracking()
            .Where(s => s.OwnerId == ownerId && !s.Deleted)
            .OrderByDescending(s => s.CreateDate)
            .Select(s => new ShopModel
            {
                ShopId = s.Id,
                Title = s.Title,
                CreatedOn = s.CreateDate
            })
            .ToSlidingCollectionWrapperAsync(parameters.Pagination);
    }
}
