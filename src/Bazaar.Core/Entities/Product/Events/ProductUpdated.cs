using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Product.Events;

public class ProductUpdated : DomainEvent
{
    public Guid? CategoryId { get; }
    public string Name { get; }
    public string Unit { get; }
    public string? Picture { get; }
    public decimal SellPrice { get; }
    public decimal BuyPrice { get; }

    public ProductUpdated(Guid aggregateRootId, Guid? categoryId,
        string name, string unit, string? picture, decimal sellPrice, decimal buyPrice)
        : base(aggregateRootId)
    {
        CategoryId = categoryId;
        Name = name;
        Unit = unit;
        Picture = picture;
        SellPrice = sellPrice;
        BuyPrice = buyPrice;
    }
}

