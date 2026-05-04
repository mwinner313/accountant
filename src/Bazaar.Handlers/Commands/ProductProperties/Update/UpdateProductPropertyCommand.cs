using Infra.Commands;

namespace Bazaar.Handlers.Commands.ProductProperties.Update;

public class UpdateProductPropertyCommand : ICommand
{
    public Guid ProductPropertyId { get; set; }
    public string Name { get; set; } = default!;
}
