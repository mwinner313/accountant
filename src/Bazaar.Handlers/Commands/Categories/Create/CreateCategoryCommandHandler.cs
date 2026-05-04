using Bazaar.Core.Entities.Category;
using Infra;
using Infra.Commands;
using Infra.EFCore;

namespace Bazaar.Handlers.Commands.Categories.Create;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCategoryCommandResult> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = new Category(command.ShopId, command.Name);
        _unitOfWork.Repo<Category>().Add(category);
        await _unitOfWork.Save(category);

        return new CreateCategoryCommandResult { CategoryId = category.Id, Name = category.Name };
    }
}
