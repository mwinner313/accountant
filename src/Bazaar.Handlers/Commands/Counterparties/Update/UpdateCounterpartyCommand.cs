using Bazaar.Handlers.Commands.Counterparties.Create;
using Infra.Commands;

namespace Bazaar.Handlers.Commands.Counterparties.Update;

public class UpdateCounterpartyCommand : ICommand
{
    public Guid CounterpartyId { get; set; }
    public string FullName { get; set; } = default!;
    public List<CounterpartyPhoneRequest> Phones { get; set; } = new();
    public List<CounterpartyBankAccountRequest> BankAccounts { get; set; } = new();
}
