using Bazaar.Core.Entities.Shop;
using Extensions.Http.Mvc;
using Infra;
using Infra.Commands;
using Infra.EFCore;

namespace Bazaar.Handlers.Commands.Shops.Create;

public class CreateShopCommandHandler : ICommandHandler<CreateShopCommand, CreateShopCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;

    public CreateShopCommandHandler(IUnitOfWork unitOfWork, IExecutionContext executionContext)
    {
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
    }

    public async Task<CreateShopCommandResult> HandleAsync(CreateShopCommand command, CancellationToken cancellationToken)
    {
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);
        var shop = new Shop(ownerId, command.Title);

        _unitOfWork.Repo<Shop>().Add(shop);
        await _unitOfWork.Save(shop);

        return new CreateShopCommandResult { ShopId = shop.Id, Title = shop.Title };
    }
}
