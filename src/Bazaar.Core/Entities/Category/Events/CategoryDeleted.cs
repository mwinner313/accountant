using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Category.Events;

public class CategoryDeleted : DomainEvent
{
    public CategoryDeleted(Guid aggregateRootId) : base(aggregateRootId) { }
}

