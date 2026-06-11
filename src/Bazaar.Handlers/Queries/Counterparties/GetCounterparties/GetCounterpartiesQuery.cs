using Extensions.Sliding;
using Infra.Queries;

namespace Bazaar.Handlers.Queries.Counterparties.GetCounterparties;

public class GetCounterpartiesQuery : IQueryResult<SlidingCollectionWrapper<CounterpartyListModel>>
{
    public string? Search { get; set; }
    public SlidingParams? Pagination { get; set; }
}
