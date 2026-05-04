using Bazaar.Data;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.ProductProperties.Delete;

public class DeleteProductPropertyCommandHandler : ICommandHandler<DeleteProductPropertyCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;

    public DeleteProductPropertyCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<Empty> HandleAsync(DeleteProductPropertyCommand command, CancellationToken cancellationToken)
    {
        var property = await _dbContext.ProductProperties
            .FirstAsync(p => p.Id == command.ProductPropertyId && !p.Deleted, cancellationToken);

        property.Delete();
        await _unitOfWork.Save(property);

        return Empty.Instance;
    }
}
