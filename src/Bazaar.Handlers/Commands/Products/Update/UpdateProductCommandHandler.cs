using Bazaar.Data;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Products.Update;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<Empty> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstAsync(p => p.Id == command.ProductId && !p.Deleted, cancellationToken);

        product.Update(command.CategoryId, command.Name, command.Unit,
            command.Picture, command.SellPrice, command.BuyPrice);

        await _unitOfWork.Save(product);

        return Empty.Instance;
    }
}
