using Extensions.Sliding;
using Infra.Queries;

namespace Bazaar.Handlers.Queries.Products.GetShopProducts;

public class GetShopProductsQuery : IQueryResult<SlidingCollectionWrapper<ProductModel>>
{
    public Guid ShopId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public SlidingParams? Pagination { get; set; }
}
