using Infra.Commands;

namespace Bazaar.Handlers.Commands.ProductProperties.Delete;

public class DeleteProductPropertyCommand : ICommand
{
    public Guid ProductPropertyId { get; set; }
}
