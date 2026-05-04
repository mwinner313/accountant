using Bazaar.Core.Entities.Shop.Events;
using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Shop;

public class Shop : AggregateRoot
{
    private Shop() { }

    public Shop(Guid ownerId, string title)
    {
        Guard.NotNullOrDefault(ownerId, nameof(ownerId));
        Guard.NotNullOrEmpty(title, nameof(title));

        var @event = new ShopCreated(Guid.NewGuid(), ownerId, title);
        ApplyChange(@event);
    }

    public void Update(string title)
    {
        Guard.NotNullOrEmpty(title, nameof(title));
        ApplyChange(new ShopUpdated(Id, title));
    }

    public void Delete()
    {
        ApplyChange(new ShopDeleted(Id));
    }

    public void Apply(ShopCreated @event)
    {
        Id = @event.AggregateRootId;
        OwnerId = @event.OwnerId;
        Title = @event.Title;
    }

    public void Apply(ShopUpdated @event)
    {
        Title = @event.Title;
    }

    public void Apply(ShopDeleted @event)
    {
        Deleted = true;
    }

    public int ShopId { get; set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = default!;
}

