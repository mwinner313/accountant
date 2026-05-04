using Infra.Commands;

namespace Bazaar.Handlers.Commands.Shops.Create;

public class CreateShopCommand : ICommand
{
    public string Title { get; set; } = default!;
}
