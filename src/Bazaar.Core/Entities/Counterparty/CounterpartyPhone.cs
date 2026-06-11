namespace Bazaar.Core.Entities.Counterparty;

public class CounterpartyPhone
{
    public int CounterpartyPhoneId { get; set; }
    public Guid CounterpartyId { get; set; }
    public string Number { get; set; } = default!;
}
