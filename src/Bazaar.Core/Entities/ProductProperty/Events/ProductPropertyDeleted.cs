using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.ProductProperty.Events;

public class ProductPropertyDeleted : DomainEvent
{
    public ProductPropertyDeleted(Guid aggregateRootId) : base(aggregateRootId) { }
}

