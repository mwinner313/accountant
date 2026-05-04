using Bazaar.Data;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Products.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDetailModel>
{
    private readonly BazaarDbContext _dbContext;

    public GetProductByIdQueryHandler(BazaarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDetailModel> HandleAsync(GetProductByIdQuery parameters, CancellationToken cts = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == parameters.ProductId && !p.Deleted)
            .Select(p => new ProductDetailModel
            {
                ProductId = p.Id,
                ShopId = p.ShopId,
                Name = p.Name,
                Unit = p.Unit,
                Picture = p.Picture,
                SellPrice = p.SellPrice,
                BuyPrice = p.BuyPrice,
                InventoryAmount = p.InventoryAmount,
                CategoryId = p.CategoryId,
                CreatedOn = p.CreateDate
            })
            .FirstAsync(cts);

        var propertyValues = await _dbContext.ProductPropertyValues
            .AsNoTracking()
            .Where(v => v.ProductId == product.ProductId && !v.Deleted)
            .Join(_dbContext.ProductProperties.Where(p => !p.Deleted),
                v => v.ProductPropertyId,
                p => p.Id,
                (v, p) => new ProductPropertyValueModel
                {
                    PropertyId = p.Id,
                    PropertyName = p.Name,
                    Value = v.Value
                })
            .ToListAsync(cts);

        product.Properties = propertyValues;

        return product;
    }
}
