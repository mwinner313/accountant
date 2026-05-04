namespace Bazaar.Core.Entities.Factor;

public class FactorItem
{
    public int FactorItemId { get; set; }
    public Guid FactorId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Amount { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime CreatedOn { get; set; }
}
