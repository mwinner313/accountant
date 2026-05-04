namespace Bazaar.Handlers.Commands.ProductProperties.Create;

public class CreateProductPropertyCommandResult
{
    public Guid ProductPropertyId { get; set; }
    public string Name { get; set; } = default!;
}
