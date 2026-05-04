using Bazaar.Handlers;
using Bazaar.Handlers.Commands.ProductProperties.Create;
using Bazaar.Handlers.Commands.ProductProperties.Delete;
using Bazaar.Handlers.Commands.ProductProperties.Update;
using Infra.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bazaar.Api.Controllers;

[ApiController]
[Route("api/shops/{shopId:guid}/product-properties")]
[Authorize]
public class ProductPropertiesController : ControllerBase
{
    private readonly ICommandProcessor _commandProcessor;

    public ProductPropertiesController(ICommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid shopId, [FromBody] CreateProductPropertyCommand command)
    {
        command.ShopId = shopId;
        var result = await _commandProcessor.ExecuteAsync<CreateProductPropertyCommand, CreateProductPropertyCommandResult>(command);
        return Ok(result);
    }

    [HttpPut("{propertyId:guid}")]
    public async Task<IActionResult> Update(Guid propertyId, [FromBody] UpdateProductPropertyCommand command)
    {
        command.ProductPropertyId = propertyId;
        await _commandProcessor.ExecuteAsync<UpdateProductPropertyCommand, Empty>(command);
        return NoContent();
    }

    [HttpDelete("{propertyId:guid}")]
    public async Task<IActionResult> Delete(Guid propertyId)
    {
        await _commandProcessor.ExecuteAsync<DeleteProductPropertyCommand, Empty>(new DeleteProductPropertyCommand { ProductPropertyId = propertyId });
        return NoContent();
    }
}
