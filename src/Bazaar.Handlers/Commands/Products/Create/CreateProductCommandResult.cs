namespace Bazaar.Handlers.Commands.Products.Create;

public class CreateProductCommandResult
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
}
