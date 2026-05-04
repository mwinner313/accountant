using Bazaar.Core.Entities.Factor;
using Bazaar.Core.Entities.Factor.Events;
using Bazaar.Data;
using Infra.Eevents;
using Infra.Events;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Handlers.EventHandlers;

public class FactorReversedEventHandler : IEventHandler<FactorReversed>
{
    private readonly BazaarDbContext _dbContext;

    public FactorReversedEventHandler(BazaarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleEvent(FactorReversed @event)
    {
        var productIds = @event.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id) && !p.Deleted)
            .ToListAsync();

        foreach (var item in @event.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product is null) continue;

            // Reverse the effect: undo what the original factor did
            if (@event.FactorType == FactorType.Buy)
                product.DecreaseInventory(item.Amount);
            else
                product.IncreaseInventory(item.Amount);
        }

        await _dbContext.SaveChangesAsync();
    }
}
