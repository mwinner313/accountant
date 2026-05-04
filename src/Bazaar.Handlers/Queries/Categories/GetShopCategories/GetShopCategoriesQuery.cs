using Infra.Queries;

namespace Bazaar.Handlers.Queries.Categories.GetShopCategories;

public class GetShopCategoriesQuery : IQueryResult<List<CategoryModel>>
{
    public Guid ShopId { get; set; }
}
