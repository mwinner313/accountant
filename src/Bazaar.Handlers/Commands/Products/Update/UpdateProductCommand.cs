using Infra.Commands;

namespace Bazaar.Handlers.Commands.Products.Update;

public class UpdateProductCommand : ICommand
{
    public Guid ProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string Unit { get; set; } = default!;
    public string? Picture { get; set; }
    public decimal SellPrice { get; set; }
    public decimal BuyPrice { get; set; }
}
