using Bazaar.Core.Entities.ProductPropertyValue;
using Bazaar.Data;
using Infra;
using Infra.Commands;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.Commands.ProductPropertyValues.Set;

public class SetProductPropertyValueCommandHandler : ICommandHandler<SetProductPropertyValueCommand, Empty>
{
    private readonly BazaarDbContext _dbContext;

    public SetProductPropertyValueCommandHandler(BazaarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Empty> HandleAsync(SetProductPropertyValueCommand command, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ProductPropertyValues
            .FirstOrDefaultAsync(v =>
                v.ProductId == command.ProductId &&
                v.ProductPropertyId == command.ProductPropertyId &&
                !v.Deleted,
                cancellationToken);

        if (existing is not null)
        {
            existing.Value = command.Value;
        }
        else
        {
            _dbContext.ProductPropertyValues.Add(new ProductPropertyValue
            {
                ProductId = command.ProductId,
                ProductPropertyId = command.ProductPropertyId,
                Value = command.Value,
                CreatedOn = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Empty.Instance;
    }
}
