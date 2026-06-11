using Bazaar.Core.Entities.Factor;
using Bazaar.Core.Entities.Factor.Events;
using Bazaar.Data;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Factors.Create;

public class CreateFactorCommandHandler : ICommandHandler<CreateFactorCommand, CreateFactorCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;

    public CreateFactorCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<CreateFactorCommandResult> HandleAsync(CreateFactorCommand command, CancellationToken cancellationToken)
    {
        var shop = await _dbContext.Shops
            .AsNoTracking()
            .FirstAsync(s => s.Id == command.ShopId && !s.Deleted, cancellationToken);

        var counterparty = await _dbContext.Counterparties
            .AsNoTracking()
            .FirstAsync(
                c => c.Id == command.CounterpartyId && c.OwnerId == shop.OwnerId && !c.Deleted,
                cancellationToken);

        var items = command.Items
            .Select(i => new FactorItemData(i.ProductId, i.Amount, i.UnitPrice))
            .ToList();

        var factor = new Factor(command.ShopId, command.Type, counterparty.Id, command.Notes, command.Date, items);

        _unitOfWork.Repo<Factor>().Add(factor);
        await _unitOfWork.Save(factor);

        return new CreateFactorCommandResult { FactorId = factor.Id };
    }
}
