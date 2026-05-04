using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Category.Events;

public class CategoryUpdated : DomainEvent
{
    public string Name { get; }

    public CategoryUpdated(Guid aggregateRootId, string name) : base(aggregateRootId)
    {
        Name = name;
    }
}

