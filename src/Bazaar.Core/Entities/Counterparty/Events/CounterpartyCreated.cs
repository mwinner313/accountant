using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Counterparty.Events;

public class CounterpartyCreated : DomainEvent
{
    public Guid OwnerId { get; }
    public string FullName { get; }
    public IReadOnlyList<CounterpartyPhoneData> Phones { get; }
    public IReadOnlyList<CounterpartyBankAccountData> BankAccounts { get; }

    public CounterpartyCreated(
        Guid aggregateRootId,
        Guid ownerId,
        string fullName,
        IReadOnlyList<CounterpartyPhoneData> phones,
        IReadOnlyList<CounterpartyBankAccountData> bankAccounts)
        : base(aggregateRootId)
    {
        OwnerId = ownerId;
        FullName = fullName;
        Phones = phones;
        BankAccounts = bankAccounts;
    }
}
