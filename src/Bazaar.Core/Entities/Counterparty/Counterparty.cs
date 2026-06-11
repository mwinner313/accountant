using Bazaar.Core.Entities.Counterparty.Events;
using Domain;
using News.Domain;

namespace Bazaar.Core.Entities.Counterparty;

public class Counterparty : AggregateRoot
{
    private Counterparty() { }

    public Counterparty(
        Guid ownerId,
        string fullName,
        IReadOnlyList<CounterpartyPhoneData> phones,
        IReadOnlyList<CounterpartyBankAccountData> bankAccounts)
    {
        Guard.NotNullOrDefault(ownerId, nameof(ownerId));
        Guard.NotNullOrEmpty(fullName, nameof(fullName));
        Guard.NotNull(phones, nameof(phones));
        Guard.NotNull(bankAccounts, nameof(bankAccounts));

        var id = Guid.NewGuid();
        ApplyChange(new CounterpartyCreated(id, ownerId, fullName, phones, bankAccounts));
    }

    public void Update(
        string fullName,
        IReadOnlyList<CounterpartyPhoneData> phones,
        IReadOnlyList<CounterpartyBankAccountData> bankAccounts)
    {
        Guard.NotNullOrEmpty(fullName, nameof(fullName));
        Guard.NotNull(phones, nameof(phones));
        Guard.NotNull(bankAccounts, nameof(bankAccounts));

        ApplyChange(new CounterpartyUpdated(Id, fullName, phones, bankAccounts));
    }

    public void Delete()
    {
        ApplyChange(new CounterpartyDeleted(Id));
    }

    public void Apply(CounterpartyCreated @event)
    {
        Id = @event.AggregateRootId;
        OwnerId = @event.OwnerId;
        FullName = @event.FullName;
        Phones = MapPhones(@event.AggregateRootId, @event.Phones);
        BankAccounts = MapBankAccounts(@event.AggregateRootId, @event.BankAccounts);
    }

    public void Apply(CounterpartyUpdated @event)
    {
        FullName = @event.FullName;
        Phones = MapPhones(Id, @event.Phones);
        BankAccounts = MapBankAccounts(Id, @event.BankAccounts);
    }

    public void Apply(CounterpartyDeleted @event)
    {
        Deleted = true;
    }

    public int CounterpartyId { get; set; }
    public Guid OwnerId { get; private set; }
    public string FullName { get; private set; } = default!;
    public List<CounterpartyPhone> Phones { get; private set; } = new();
    public List<CounterpartyBankAccount> BankAccounts { get; private set; } = new();

    private static List<CounterpartyPhone> MapPhones(Guid counterpartyId, IReadOnlyList<CounterpartyPhoneData> phones)
    {
        return phones
            .Select(p => new CounterpartyPhone
            {
                CounterpartyId = counterpartyId,
                Number = p.Number
            })
            .ToList();
    }

    private static List<CounterpartyBankAccount> MapBankAccounts(
        Guid counterpartyId,
        IReadOnlyList<CounterpartyBankAccountData> accounts)
    {
        return accounts
            .Select(a => new CounterpartyBankAccount
            {
                CounterpartyId = counterpartyId,
                Name = a.Name,
                AccountNumber = a.AccountNumber,
                ShebaNumber = a.ShebaNumber,
                CardNumber = a.CardNumber
            })
            .ToList();
    }
}
