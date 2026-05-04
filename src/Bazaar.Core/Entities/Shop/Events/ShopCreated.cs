using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Shop.Events;

public class ShopCreated : DomainEvent
{
    public Guid OwnerId { get; }
    public string Title { get; }

    public ShopCreated(Guid aggregateRootId, Guid ownerId, string title) : base(aggregateRootId)
    {
        OwnerId = ownerId;
        Title = title;
    }
}

