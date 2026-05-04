using Bazaar.Core.Entities.ProductProperty.Events;
using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.ProductProperty;

public class ProductProperty : AggregateRoot
{
    private ProductProperty() { }

    public ProductProperty(Guid shopId, string name)
    {
        Guard.NotNullOrDefault(shopId, nameof(shopId));
        Guard.NotNullOrEmpty(name, nameof(name));

        var @event = new ProductPropertyCreated(Guid.NewGuid(), shopId, name);
        ApplyChange(@event);
    }

    public void Update(string name)
    {
        Guard.NotNullOrEmpty(name, nameof(name));
        ApplyChange(new ProductPropertyUpdated(Id, name));
    }

    public void Delete()
    {
        ApplyChange(new ProductPropertyDeleted(Id));
    }

    public void Apply(ProductPropertyCreated @event)
    {
        Id = @event.AggregateRootId;
        ShopId = @event.ShopId;
        Name = @event.Name;
    }

    public void Apply(ProductPropertyUpdated @event)
    {
        Name = @event.Name;
    }

    public void Apply(ProductPropertyDeleted @event)
    {
        Deleted = true;
    }

    public int ProductPropertyId { get; set; }
    public Guid ShopId { get; private set; }
    public string Name { get; private set; } = default!;
}

