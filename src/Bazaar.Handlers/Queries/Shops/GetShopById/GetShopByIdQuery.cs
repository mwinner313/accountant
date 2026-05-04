using Infra.Queries;

namespace Bazaar.Handlers.Queries.Shops.GetShopById;

public class GetShopByIdQuery : IQueryResult<ShopDetailModel>
{
    public Guid ShopId { get; set; }
}
