using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Shop.Events;

public class ShopDeleted : DomainEvent
{
    public ShopDeleted(Guid aggregateRootId) : base(aggregateRootId) { }
}

