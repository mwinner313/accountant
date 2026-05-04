using Infra.Commands;

namespace Bazaar.Handlers.Commands.Categories.Update;

public class UpdateCategoryCommand : ICommand
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
}
