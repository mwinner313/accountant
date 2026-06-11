using Bazaar.Core.Entities.Factor;

namespace Bazaar.Handlers.Queries.Factors.GetFactorById;

public class FactorDetailModel
{
    public Guid FactorId { get; set; }
    public Guid ShopId { get; set; }
    public Guid? CounterpartyId { get; set; }
    public string? CounterpartyFullName { get; set; }
    public FactorType Type { get; set; }
    public string? Notes { get; set; }
    public DateTime Date { get; set; }
    public bool IsReversed { get; set; }
    public DateTime CreatedOn { get; set; }
    public List<FactorItemDetailModel> Items { get; set; } = new();
}

public class FactorItemDetailModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Amount * UnitPrice;
}
