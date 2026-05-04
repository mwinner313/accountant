using Bazaar.Core.Entities.Category.Events;
using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Category;

public class Category : AggregateRoot
{
    private Category() { }

    public Category(Guid shopId, string name)
    {
        Guard.NotNullOrDefault(shopId, nameof(shopId));
        Guard.NotNullOrEmpty(name, nameof(name));

        var @event = new CategoryCreated(Guid.NewGuid(), shopId, name);
        ApplyChange(@event);
    }

    public void Update(string name)
    {
        Guard.NotNullOrEmpty(name, nameof(name));
        ApplyChange(new CategoryUpdated(Id, name));
    }

    public void Delete()
    {
        ApplyChange(new CategoryDeleted(Id));
    }

    public void Apply(CategoryCreated @event)
    {
        Id = @event.AggregateRootId;
        ShopId = @event.ShopId;
        Name = @event.Name;
    }

    public void Apply(CategoryUpdated @event)
    {
        Name = @event.Name;
    }

    public void Apply(CategoryDeleted @event)
    {
        Deleted = true;
    }

    public int CategoryId { get; set; }
    public Guid ShopId { get; private set; }
    public string Name { get; private set; } = default!;
}

