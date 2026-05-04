using Bazaar.Core.Entities.Shop;
using Bazaar.Data;
using Extensions.Http.Mvc;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Shops.Update;

public class UpdateShopCommandHandler : ICommandHandler<UpdateShopCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;
    private readonly IExecutionContext _executionContext;

    public UpdateShopCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext, IExecutionContext executionContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _executionContext = executionContext;
    }

    public async Task<Empty> HandleAsync(UpdateShopCommand command, CancellationToken cancellationToken)
    {
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);
        var shop = await _dbContext.Shops
            .FirstAsync(s => s.Id == command.ShopId && s.OwnerId == ownerId && !s.Deleted, cancellationToken);

        shop.Update(command.Title);
        await _unitOfWork.Save(shop);

        return Empty.Instance;
    }
}
