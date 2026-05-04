using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Product.Events;

public class ProductCreated : DomainEvent
{
    public Guid ShopId { get; }
    public Guid? CategoryId { get; }
    public string Name { get; }
    public string Unit { get; }
    public string? Picture { get; }
    public decimal SellPrice { get; }
    public decimal BuyPrice { get; }

    public ProductCreated(Guid aggregateRootId, Guid shopId, Guid? categoryId,
        string name, string unit, string? picture, decimal sellPrice, decimal buyPrice)
        : base(aggregateRootId)
    {
        ShopId = shopId;
        CategoryId = categoryId;
        Name = name;
        Unit = unit;
        Picture = picture;
        SellPrice = sellPrice;
        BuyPrice = buyPrice;
    }
}

