using Infra.Commands;

namespace Bazaar.Handlers.Commands.Categories.Delete;

public class DeleteCategoryCommand : ICommand
{
    public Guid CategoryId { get; set; }
}
