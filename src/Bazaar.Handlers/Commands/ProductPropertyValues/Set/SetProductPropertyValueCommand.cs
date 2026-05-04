using Infra.Commands;

namespace Bazaar.Handlers.Commands.ProductPropertyValues.Set;

public class SetProductPropertyValueCommand : ICommand
{
    public Guid ProductId { get; set; }
    public Guid ProductPropertyId { get; set; }
    public string Value { get; set; } = default!;
}
