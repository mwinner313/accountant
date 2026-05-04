namespace Bazaar.Handlers.Queries.Products.GetProductById;

public class ProductDetailModel
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    public string Name { get; set; } = default!;
    public string Unit { get; set; } = default!;
    public string? Picture { get; set; }
    public decimal SellPrice { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal InventoryAmount { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime CreatedOn { get; set; }
    public List<ProductPropertyValueModel> Properties { get; set; } = new();
}

public class ProductPropertyValueModel
{
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public string Value { get; set; } = default!;
}
