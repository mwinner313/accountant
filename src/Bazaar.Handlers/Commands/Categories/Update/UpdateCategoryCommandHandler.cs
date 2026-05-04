using Bazaar.Data;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Categories.Update;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<Empty> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstAsync(c => c.Id == command.CategoryId && !c.Deleted, cancellationToken);

        category.Update(command.Name);
        await _unitOfWork.Save(category);

        return Empty.Instance;
    }
}
