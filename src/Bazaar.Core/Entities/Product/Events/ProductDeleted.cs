using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Product.Events;

public class ProductDeleted : DomainEvent
{
    public ProductDeleted(Guid aggregateRootId) : base(aggregateRootId) { }
}

