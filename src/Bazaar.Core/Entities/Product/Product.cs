using Bazaar.Core.Entities.Product.Events;
using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Product;

public class Product : AggregateRoot
{
    private Product() { }

    public Product(Guid shopId, Guid? categoryId, string name, string unit,
        string? picture, decimal sellPrice, decimal buyPrice)
    {
        Guard.NotNullOrDefault(shopId, nameof(shopId));
        Guard.NotNullOrEmpty(name, nameof(name));
        Guard.NotNullOrEmpty(unit, nameof(unit));

        var @event = new ProductCreated(Guid.NewGuid(), shopId, categoryId,
            name, unit, picture, sellPrice, buyPrice);
        ApplyChange(@event);
    }

    public void Update(Guid? categoryId, string name, string unit,
        string? picture, decimal sellPrice, decimal buyPrice)
    {
        Guard.NotNullOrEmpty(name, nameof(name));
        Guard.NotNullOrEmpty(unit, nameof(unit));

        ApplyChange(new ProductUpdated(Id, categoryId, name, unit, picture, sellPrice, buyPrice));
    }

    public void Delete()
    {
        ApplyChange(new ProductDeleted(Id));
    }

    public void IncreaseInventory(decimal amount)
    {
        InventoryAmount += amount;
    }

    public void DecreaseInventory(decimal amount)
    {
        InventoryAmount -= amount;
    }

    public void Apply(ProductCreated @event)
    {
        Id = @event.AggregateRootId;
        ShopId = @event.ShopId;
        CategoryId = @event.CategoryId;
        Name = @event.Name;
        Unit = @event.Unit;
        Picture = @event.Picture;
        SellPrice = @event.SellPrice;
        BuyPrice = @event.BuyPrice;
        InventoryAmount = 0;
    }

    public void Apply(ProductUpdated @event)
    {
        CategoryId = @event.CategoryId;
        Name = @event.Name;
        Unit = @event.Unit;
        Picture = @event.Picture;
        SellPrice = @event.SellPrice;
        BuyPrice = @event.BuyPrice;
    }

    public void Apply(ProductDeleted @event)
    {
        Deleted = true;
    }

    public int ProductId { get; set; }
    public Guid ShopId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Unit { get; private set; } = default!;
    public string? Picture { get; private set; }
    public decimal SellPrice { get; private set; }
    public decimal BuyPrice { get; private set; }
    public decimal InventoryAmount { get; private set; }
}

