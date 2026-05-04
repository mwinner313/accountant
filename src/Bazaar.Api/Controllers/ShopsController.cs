using Bazaar.Handlers;
using Bazaar.Handlers.Commands.Shops.Create;
using Bazaar.Handlers.Commands.Shops.Delete;
using Bazaar.Handlers.Commands.Shops.Update;
using Bazaar.Handlers.Queries.Shops.GetMyShops;
using Bazaar.Handlers.Queries.Shops.GetShopById;
using Extensions.Sliding;
using Infra.Commands;
using Infra.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShopsController : ControllerBase
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    public ShopsController(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyShops([FromQuery] GetMyShopsQuery query)
    {
        var result = await _queryProcessor.ExecuteAsync(query);
        return Ok(result);
    }

    [HttpGet("{shopId:guid}")]
    public async Task<IActionResult> GetById(Guid shopId)
    {
        var result = await _queryProcessor.ExecuteAsync(new GetShopByIdQuery { ShopId = shopId });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShopCommand command)
    {
        var result = await _commandProcessor.ExecuteAsync<CreateShopCommand, CreateShopCommandResult>(command);
        return Ok(result);
    }

    [HttpPut("{shopId:guid}")]
    public async Task<IActionResult> Update(Guid shopId, [FromBody] UpdateShopCommand command)
    {
        command.ShopId = shopId;
        await _commandProcessor.ExecuteAsync<UpdateShopCommand, Empty>(command);
        return NoContent();
    }

    [HttpDelete("{shopId:guid}")]
    public async Task<IActionResult> Delete(Guid shopId)
    {
        await _commandProcessor.ExecuteAsync<DeleteShopCommand, Empty>(new DeleteShopCommand { ShopId = shopId });
        return NoContent();
    }
}
