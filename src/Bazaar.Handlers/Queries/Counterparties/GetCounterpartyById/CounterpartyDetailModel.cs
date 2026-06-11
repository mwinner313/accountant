namespace Bazaar.Handlers.Queries.Counterparties.GetCounterpartyById;

public class CounterpartyDetailModel
{
    public Guid CounterpartyId { get; set; }
    public string FullName { get; set; } = default!;
    public List<CounterpartyPhoneModel> Phones { get; set; } = new();
    public List<CounterpartyBankAccountModel> BankAccounts { get; set; } = new();
}

public class CounterpartyPhoneModel
{
    public string Number { get; set; } = default!;
}

public class CounterpartyBankAccountModel
{
    public string Name { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public string ShebaNumber { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
}
