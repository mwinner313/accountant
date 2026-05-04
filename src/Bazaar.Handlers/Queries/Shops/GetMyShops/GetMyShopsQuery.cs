using Extensions.Sliding;
using Infra.Queries;

namespace Bazaar.Handlers.Queries.Shops.GetMyShops;

public class GetMyShopsQuery : IQueryResult<SlidingCollectionWrapper<ShopModel>>
{
    public SlidingParams? Pagination { get; set; }
}
