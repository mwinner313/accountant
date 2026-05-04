using Infra.Queries;

namespace Bazaar.Handlers.Queries.Products.GetProductById;

public class GetProductByIdQuery : IQueryResult<ProductDetailModel>
{
    public Guid ProductId { get; set; }
}
