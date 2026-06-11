namespace Bazaar.Core.Entities.Counterparty;

public class CounterpartyBankAccount
{
    public int CounterpartyBankAccountId { get; set; }
    public Guid CounterpartyId { get; set; }
    public string Name { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public string ShebaNumber { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
}
