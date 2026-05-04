using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.ProductProperty.Events;

public class ProductPropertyUpdated : DomainEvent
{
    public string Name { get; }

    public ProductPropertyUpdated(Guid aggregateRootId, string name) : base(aggregateRootId)
    {
        Name = name;
    }
}

