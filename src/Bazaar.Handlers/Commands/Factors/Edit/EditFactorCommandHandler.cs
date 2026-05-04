using Bazaar.Core.Entities.Factor;
using Bazaar.Core.Entities.Factor.Events;
using Bazaar.Data;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Factors.Edit;

public class EditFactorCommandHandler : ICommandHandler<EditFactorCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;

    public EditFactorCommandHandler(IUnitOfWork unitOfWork, BazaarDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<Empty> HandleAsync(EditFactorCommand command, CancellationToken cancellationToken)
    {
        var original = await _dbContext.Factors
            .Include(f => f.Items)
            .FirstAsync(f => f.Id == command.FactorId && !f.Deleted, cancellationToken);

        // Step 1: reverse the original factor (raises FactorReversed → undoes inventory)
        original.Reverse();
        await _unitOfWork.Save(original);

        // Step 2: create a new factor with the updated values (raises FactorCreated → applies inventory)
        var newItems = command.Items
            .Select(i => new FactorItemData(i.ProductId, i.Amount, i.UnitPrice))
            .ToList();

        var newFactor = new Factor(original.ShopId, original.Type, command.Notes, command.Date, newItems);
        _unitOfWork.Repo<Factor>().Add(newFactor);
        await _unitOfWork.Save(newFactor);

        return Empty.Instance;
    }
}
