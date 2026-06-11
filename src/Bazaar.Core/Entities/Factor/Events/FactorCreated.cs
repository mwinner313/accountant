using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Factor.Events;

public class FactorCreated : DomainEvent
{
    public Guid ShopId { get; }
    public FactorType FactorType { get; }
    public string? Notes { get; }
    public DateTime Date { get; }
    public List<FactorItemData> Items { get; }

    public Guid CounterpartyId { get; }

    public FactorCreated(Guid aggregateRootId, Guid shopId, FactorType factorType,
        Guid counterpartyId, string? notes, DateTime date, List<FactorItemData> items)
        : base(aggregateRootId)
    {
        ShopId = shopId;
        FactorType = factorType;
        CounterpartyId = counterpartyId;
        Notes = notes;
        Date = date;
        Items = items;
    }
}

public record FactorItemData(Guid ProductId, decimal Amount, decimal UnitPrice);

