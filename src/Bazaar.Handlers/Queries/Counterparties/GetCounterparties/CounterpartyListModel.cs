namespace Bazaar.Handlers.Queries.Counterparties.GetCounterparties;

public class CounterpartyListModel
{
    public Guid CounterpartyId { get; set; }
    public string FullName { get; set; } = default!;
}
