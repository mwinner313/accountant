namespace Bazaar.Core.Entities.Counterparty.Events;

public record CounterpartyBankAccountData(
    string Name,
    string AccountNumber,
    string ShebaNumber,
    string CardNumber);
