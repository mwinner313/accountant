using Infra.Commands;

namespace Bazaar.Handlers.Commands.ProductProperties.Create;

public class CreateProductPropertyCommand : ICommand
{
    public Guid ShopId { get; set; }
    public string Name { get; set; } = default!;
}
