using Bazaar.Data;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Products.Delete;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<Empty> HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstAsync(p => p.Id == command.ProductId && !p.Deleted, cancellationToken);

        product.Delete();
        await _unitOfWork.Save(product);

        return Empty.Instance;
    }
}
