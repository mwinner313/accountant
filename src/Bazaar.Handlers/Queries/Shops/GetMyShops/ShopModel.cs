namespace Bazaar.Handlers.Queries.Shops.GetMyShops;

public class ShopModel
{
    public Guid ShopId { get; set; }
    public string Title { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
}
