using Bazaar.Data;
using Extensions.Sliding;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Products.GetShopProducts;

public class GetShopProductsQueryHandler : IQueryHandler<GetShopProductsQuery, SlidingCollectionWrapper<ProductModel>>
{
    private readonly BazaarDbContext _dbContext;

    public GetShopProductsQueryHandler(BazaarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SlidingCollectionWrapper<ProductModel>> HandleAsync(GetShopProductsQuery parameters, CancellationToken cts = default)
    {
        parameters.Pagination ??= new SlidingParams { Skip = 0, Take = 20 };

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.ShopId == parameters.ShopId && !p.Deleted);

        if (parameters.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == parameters.CategoryId);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
            query = query.Where(p => p.Name.Contains(parameters.Search));

        return query
            .OrderByDescending(p => p.CreateDate)
            .Select(p => new ProductModel
            {
                ProductId = p.Id,
                Name = p.Name,
                Unit = p.Unit,
                Picture = p.Picture,
                SellPrice = p.SellPrice,
                BuyPrice = p.BuyPrice,
                InventoryAmount = p.InventoryAmount,
                CategoryId = p.CategoryId
            })
            .ToSlidingCollectionWrapperAsync(parameters.Pagination);
    }
}
