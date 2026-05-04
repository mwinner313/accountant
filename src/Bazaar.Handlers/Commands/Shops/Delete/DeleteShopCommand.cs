using Infra.Commands;

namespace Bazaar.Handlers.Commands.Shops.Delete;

public class DeleteShopCommand : ICommand
{
    public Guid ShopId { get; set; }
}
