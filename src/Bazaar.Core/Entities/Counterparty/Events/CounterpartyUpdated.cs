using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Counterparty.Events;

public class CounterpartyUpdated : DomainEvent
{
    public string FullName { get; }
    public IReadOnlyList<CounterpartyPhoneData> Phones { get; }
    public IReadOnlyList<CounterpartyBankAccountData> BankAccounts { get; }

    public CounterpartyUpdated(
        Guid aggregateRootId,
        string fullName,
        IReadOnlyList<CounterpartyPhoneData> phones,
        IReadOnlyList<CounterpartyBankAccountData> bankAccounts)
        : base(aggregateRootId)
    {
        FullName = fullName;
        Phones = phones;
        BankAccounts = bankAccounts;
    }
}
