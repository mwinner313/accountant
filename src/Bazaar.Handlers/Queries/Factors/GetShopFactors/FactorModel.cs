using Bazaar.Core.Entities.Factor;

namespace Bazaar.Handlers.Queries.Factors.GetShopFactors;

public class FactorModel
{
    public Guid FactorId { get; set; }
    public FactorType Type { get; set; }
    public string? Notes { get; set; }
    public DateTime Date { get; set; }
    public bool IsReversed { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedOn { get; set; }
}
