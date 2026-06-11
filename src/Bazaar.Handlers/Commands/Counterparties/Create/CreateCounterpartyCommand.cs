using Infra.Commands;

namespace Bazaar.Handlers.Commands.Counterparties.Create;

public class CreateCounterpartyCommand : ICommand
{
    public string FullName { get; set; } = default!;
    public List<CounterpartyPhoneRequest> Phones { get; set; } = new();
    public List<CounterpartyBankAccountRequest> BankAccounts { get; set; } = new();
}

public class CounterpartyPhoneRequest
{
    public string Number { get; set; } = default!;
}

public class CounterpartyBankAccountRequest
{
    public string Name { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public string ShebaNumber { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
}
