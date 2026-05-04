using Bazaar.Data;
using Extensions.Http.Mvc;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Shops.Delete;

public class DeleteShopCommandHandler : ICommandHandler<DeleteShopCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;
    private readonly IExecutionContext _executionContext;

    public DeleteShopCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext, IExecutionContext executionContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _executionContext = executionContext;
    }

    public async Task<Empty> HandleAsync(DeleteShopCommand command, CancellationToken cancellationToken)
    {
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);
        var shop = await _dbContext.Shops
            .FirstAsync(s => s.Id == command.ShopId && s.OwnerId == ownerId && !s.Deleted, cancellationToken);

        shop.Delete();
        await _unitOfWork.Save(shop);

        return Empty.Instance;
    }
}
