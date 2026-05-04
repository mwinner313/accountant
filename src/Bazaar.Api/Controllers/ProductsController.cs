using Bazaar.Handlers;
using Bazaar.Handlers.Commands.Products.Create;
using Bazaar.Handlers.Commands.Products.Delete;
using Bazaar.Handlers.Commands.Products.Update;
using Bazaar.Handlers.Commands.ProductPropertyValues.Set;
using Bazaar.Handlers.Queries.Products.GetProductById;
using Bazaar.Handlers.Queries.Products.GetShopProducts;
using Extensions.Sliding;
using Infra.Commands;
using Infra.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bazaar.Api.Controllers;

[ApiController]
[Route("api/shops/{shopId:guid}/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    public ProductsController(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid shopId, [FromQuery] GetShopProductsQuery query)
    {
        query.ShopId = shopId;
        var result = await _queryProcessor.ExecuteAsync(query);
        return Ok(result);
    }

    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetById(Guid productId)
    {
        var result = await _queryProcessor.ExecuteAsync(new GetProductByIdQuery { ProductId = productId });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid shopId, [FromBody] CreateProductCommand command)
    {
        command.ShopId = shopId;
        var result = await _commandProcessor.ExecuteAsync<CreateProductCommand, CreateProductCommandResult>(command);
        return Ok(result);
    }

    [HttpPut("{productId:guid}")]
    public async Task<IActionResult> Update(Guid productId, [FromBody] UpdateProductCommand command)
    {
        command.ProductId = productId;
        await _commandProcessor.ExecuteAsync<UpdateProductCommand, Empty>(command);
        return NoContent();
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Delete(Guid productId)
    {
        await _commandProcessor.ExecuteAsync<DeleteProductCommand, Empty>(new DeleteProductCommand { ProductId = productId });
        return NoContent();
    }

    [HttpPost("{productId:guid}/properties")]
    public async Task<IActionResult> SetPropertyValue(Guid productId, [FromBody] SetProductPropertyValueCommand command)
    {
        command.ProductId = productId;
        await _commandProcessor.ExecuteAsync<SetProductPropertyValueCommand, Empty>(command);
        return NoContent();
    }
}
