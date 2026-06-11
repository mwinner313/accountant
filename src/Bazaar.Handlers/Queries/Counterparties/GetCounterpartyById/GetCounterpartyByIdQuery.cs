using Infra.Queries;

namespace Bazaar.Handlers.Queries.Counterparties.GetCounterpartyById;

public class GetCounterpartyByIdQuery : IQueryResult<CounterpartyDetailModel>
{
    public Guid CounterpartyId { get; set; }
}
