namespace Bazaar.Handlers.Commands.Shops.Create;

public class CreateShopCommandResult
{
    public Guid ShopId { get; set; }
    public string Title { get; set; } = default!;
}
