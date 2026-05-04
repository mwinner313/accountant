using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Category.Events;

public class CategoryCreated : DomainEvent
{
    public Guid ShopId { get; }
    public string Name { get; }

    public CategoryCreated(Guid aggregateRootId, Guid shopId, string name) : base(aggregateRootId)
    {
        ShopId = shopId;
        Name = name;
    }
}

