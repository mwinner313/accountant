using Bazaar.Handlers;
using Bazaar.Handlers.Commands.Factors.Create;
using Bazaar.Handlers.Commands.Factors.Edit;
using Bazaar.Handlers.Queries.Factors.GetFactorById;
using Bazaar.Handlers.Queries.Factors.GetShopFactors;
using Extensions.Sliding;
using Infra.Commands;
using Infra.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bazaar.Api.Controllers;

[ApiController]
[Route("api/shops/{shopId:guid}/[controller]")]
[Authorize]
public class FactorsController : ControllerBase
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    public FactorsController(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid shopId, [FromQuery] GetShopFactorsQuery query)
    {
        query.ShopId = shopId;
        var result = await _queryProcessor.ExecuteAsync(query);
        return Ok(result);
    }

    [HttpGet("{factorId:guid}")]
    public async Task<IActionResult> GetById(Guid factorId)
    {
        var result = await _queryProcessor.ExecuteAsync(new GetFactorByIdQuery { FactorId = factorId });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid shopId, [FromBody] CreateFactorCommand command)
    {
        command.ShopId = shopId;
        var result = await _commandProcessor.ExecuteAsync<CreateFactorCommand, CreateFactorCommandResult>(command);
        return Ok(result);
    }

    [HttpPut("{factorId:guid}")]
    public async Task<IActionResult> Edit(Guid factorId, [FromBody] EditFactorCommand command)
    {
        command.FactorId = factorId;
        await _commandProcessor.ExecuteAsync<EditFactorCommand, Empty>(command);
        return NoContent();
    }
}
