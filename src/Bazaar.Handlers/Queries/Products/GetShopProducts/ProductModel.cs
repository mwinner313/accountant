namespace Bazaar.Handlers.Queries.Products.GetShopProducts;

public class ProductModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string Unit { get; set; } = default!;
    public string? Picture { get; set; }
    public decimal SellPrice { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal InventoryAmount { get; set; }
    public Guid? CategoryId { get; set; }
}
