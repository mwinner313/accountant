using Infra.Queries;

namespace Bazaar.Handlers.Queries.Factors.GetFactorById;

public class GetFactorByIdQuery : IQueryResult<FactorDetailModel>
{
    public Guid FactorId { get; set; }
}
