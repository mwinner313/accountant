using Infra.Commands;

namespace Bazaar.Handlers.Commands.Products.Delete;

public class DeleteProductCommand : ICommand
{
    public Guid ProductId { get; set; }
}
