using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Shop.Events;

public class ShopUpdated : DomainEvent
{
    public string Title { get; }

    public ShopUpdated(Guid aggregateRootId, string title) : base(aggregateRootId)
    {
        Title = title;
    }
}

