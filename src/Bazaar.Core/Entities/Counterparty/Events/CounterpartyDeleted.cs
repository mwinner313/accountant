using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Counterparty.Events;

public class CounterpartyDeleted : DomainEvent
{
    public CounterpartyDeleted(Guid aggregateRootId) : base(aggregateRootId)
    {
    }
}
