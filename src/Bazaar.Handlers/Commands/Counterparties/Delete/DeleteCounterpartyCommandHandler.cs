using Bazaar.Core.Entities.Counterparty;
using Bazaar.Data;
using Extensions.Http.Mvc;
using Infra;
using Infra.Commands;
using Infra.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.Counterparties.Delete;

public class DeleteCounterpartyCommandHandler : ICommandHandler<DeleteCounterpartyCommand, Empty>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BazaarDbContext _dbContext;
    private readonly IExecutionContext _executionContext;

    public DeleteCounterpartyCommandHandler(
        IUnitOfWork unitOfWork,
        BazaarDbContext dbContext,
        IExecutionContext executionContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _executionContext = executionContext;
    }

    public async Task<Empty> HandleAsync(DeleteCounterpartyCommand command, CancellationToken cancellationToken)
    {
        var ownerId = Guid.Parse(_executionContext.CurrentUserId);
        var hasFactors = await _dbContext.Factors
            .AnyAsync(f => f.CounterpartyId == command.CounterpartyId && !f.Deleted, cancellationToken);

        if (hasFactors)
            throw new InvalidOperationException("Cannot delete a counterparty that is referenced by factors.");

        var counterparty = await _dbContext.Counterparties
            .FirstAsync(
                c => c.Id == command.CounterpartyId && c.OwnerId == ownerId && !c.Deleted,
                cancellationToken);

        counterparty.Delete();
        await _unitOfWork.Save(counterparty);

        return Empty.Instance;
    }
}
