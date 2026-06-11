using Infra.Commands;

namespace Bazaar.Handlers.Commands.Counterparties.Delete;

public class DeleteCounterpartyCommand : ICommand
{
    public Guid CounterpartyId { get; set; }
}
