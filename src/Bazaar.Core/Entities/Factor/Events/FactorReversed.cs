using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Factor.Events;

public class FactorReversed : DomainEvent
{
    public Guid ShopId { get; }
    public FactorType FactorType { get; }
    public List<FactorItemData> Items { get; }

    public FactorReversed(Guid aggregateRootId, Guid shopId, FactorType factorType, List<FactorItemData> items)
        : base(aggregateRootId)
    {
        ShopId = shopId;
        FactorType = factorType;
        Items = items;
    }
}

