using Bazaar.Core.Entities.Factor;
using Extensions.Sliding;
using Infra.Queries;

namespace Bazaar.Handlers.Queries.Factors.GetShopFactors;

public class GetShopFactorsQuery : IQueryResult<SlidingCollectionWrapper<FactorModel>>
{
    public Guid ShopId { get; set; }
    public FactorType? Type { get; set; }
    public SlidingParams? Pagination { get; set; }
}
