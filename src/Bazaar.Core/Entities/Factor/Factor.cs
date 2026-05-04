using Bazaar.Core.Entities.Factor.Events;
using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Factor;

public class Factor : AggregateRoot
{
    private Factor() { }

    public Factor(Guid shopId, FactorType type, string? notes, DateTime date,
        List<FactorItemData> items)
    {
        Guard.NotNullOrDefault(shopId, nameof(shopId));
        Guard.NotNull(items, nameof(items));

        if (items.Count == 0)
            throw new ArgumentException("Factor must have at least one item.", nameof(items));

        var id = Guid.NewGuid();
        var @event = new FactorCreated(id, shopId, type, notes, date, items);
        ApplyChange(@event);
    }

    public void Reverse()
    {
        if (IsReversed)
            throw new InvalidOperationException("Factor is already reversed.");

        var itemData = Items
            .Select(i => new FactorItemData(i.ProductId, i.Amount, i.UnitPrice))
            .ToList();

        ApplyChange(new FactorReversed(Id, ShopId, Type, itemData));
    }

    public void Apply(FactorCreated @event)
    {
        Id = @event.AggregateRootId;
        ShopId = @event.ShopId;
        Type = @event.FactorType;
        Notes = @event.Notes;
        Date = @event.Date;
        IsReversed = false;
        Items = @event.Items.Select(i => new FactorItem
        {
            FactorId = @event.AggregateRootId,
            ProductId = i.ProductId,
            Amount = i.Amount,
            UnitPrice = i.UnitPrice,
            CreatedOn = DateTime.UtcNow
        }).ToList();
    }

    public void Apply(FactorReversed @event)
    {
        IsReversed = true;
    }

    public int FactorId { get; set; }
    public Guid ShopId { get; private set; }
    public FactorType Type { get; private set; }
    public string? Notes { get; private set; }
    public DateTime Date { get; private set; }
    public bool IsReversed { get; private set; }
    public List<FactorItem> Items { get; private set; } = new();
}

