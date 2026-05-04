using Infra.Commands;

namespace Bazaar.Handlers.Commands.Categories.Create;

public class CreateCategoryCommand : ICommand
{
    public Guid ShopId { get; set; }
    public string Name { get; set; } = default!;
}
