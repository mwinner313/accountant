using Bazaar.Core.Entities.Factor;
using Infra.Commands;

namespace Bazaar.Handlers.Commands.Factors.Create;

public class CreateFactorCommand : ICommand
{
    public Guid ShopId { get; set; }
    public Guid CounterpartyId { get; set; }
    public FactorType Type { get; set; }
    public string? Notes { get; set; }
    public DateTime Date { get; set; }
    public List<FactorItemRequest> Items { get; set; } = new();
}

public class FactorItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Amount { get; set; }
    public decimal UnitPrice { get; set; }
}
