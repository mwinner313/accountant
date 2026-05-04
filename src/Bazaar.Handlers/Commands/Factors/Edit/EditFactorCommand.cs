using Bazaar.Core.Entities.Factor;
using Infra.Commands;

namespace Bazaar.Handlers.Commands.Factors.Edit;

public class EditFactorCommand : ICommand
{
    public Guid FactorId { get; set; }
    public string? Notes { get; set; }
    public DateTime Date { get; set; }
    public List<EditFactorItemRequest> Items { get; set; } = new();
}

public class EditFactorItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Amount { get; set; }
    public decimal UnitPrice { get; set; }
}
