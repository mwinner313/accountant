using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.ProductProperty.Events;

public class ProductPropertyCreated : DomainEvent
{
    public Guid ShopId { get; }
    public string Name { get; }

    public ProductPropertyCreated(Guid aggregateRootId, Guid shopId, string name) : base(aggregateRootId)
    {
        ShopId = shopId;
        Name = name;
    }
}

