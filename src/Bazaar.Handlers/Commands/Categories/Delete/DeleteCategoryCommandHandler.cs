using Bazaar.Data;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Categories.Delete;

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<Empty> HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstAsync(c => c.Id == command.CategoryId && !c.Deleted, cancellationToken);

        category.Delete();
        await _unitOfWork.Save(category);

        return Empty.Instance;
    }
}
