namespace Bazaar.Handlers.Commands.Categories.Create;

public class CreateCategoryCommandResult
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
}
