using Bazaar.Data;
using Infra.Queries;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Queries.Categories.GetShopCategories;

public class GetShopCategoriesQueryHandler : IQueryHandler<GetShopCategoriesQuery, List<CategoryModel>>
{
    private readonly BazaarDbContext _dbContext;

    public GetShopCategoriesQueryHandler(BazaarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<CategoryModel>> HandleAsync(GetShopCategoriesQuery parameters, CancellationToken cts = default)
    {
        return _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.ShopId == parameters.ShopId && !c.Deleted)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryModel { CategoryId = c.Id, Name = c.Name })
            .ToListAsync(cts);
    }
}
