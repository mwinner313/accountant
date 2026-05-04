using Infra.Commands;

namespace Bazaar.Handlers.Commands.Shops.Update;

public class UpdateShopCommand : ICommand
{
    public Guid ShopId { get; set; }
    public string Title { get; set; } = default!;
}
