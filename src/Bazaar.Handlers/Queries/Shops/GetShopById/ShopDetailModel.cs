namespace Bazaar.Handlers.Queries.Shops.GetShopById;

public class ShopDetailModel
{
    public Guid ShopId { get; set; }
    public string Title { get; set; } = default!;
    public Guid OwnerId { get; set; }
    public DateTime CreatedOn { get; set; }
}
