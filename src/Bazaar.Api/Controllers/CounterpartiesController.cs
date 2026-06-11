using Bazaar.Handlers;
using Bazaar.Handlers.Commands.Counterparties.Create;
using Bazaar.Handlers.Commands.Counterparties.Delete;
using Bazaar.Handlers.Commands.Counterparties.Update;
using Bazaar.Handlers.Queries.Counterparties.GetCounterparties;
using Bazaar.Handlers.Queries.Counterparties.GetCounterpartyById;
using Infra;
using Infra.Commands;
using Infra.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CounterpartiesController : ControllerBase
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    public CounterpartiesController(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetCounterpartiesQuery query)
    {
        var result = await _queryProcessor.ExecuteAsync(query);
        return Ok(result);
    }

    [HttpGet("{counterpartyId:guid}")]
    public async Task<IActionResult> GetById(Guid counterpartyId)
    {
        var result = await _queryProcessor.ExecuteAsync(new GetCounterpartyByIdQuery { CounterpartyId = counterpartyId });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCounterpartyCommand command)
    {
        var result = await _commandProcessor.ExecuteAsync<CreateCounterpartyCommand, CreateCounterpartyCommandResult>(command);
        return Ok(result);
    }

    [HttpPut("{counterpartyId:guid}")]
    public async Task<IActionResult> Update(Guid counterpartyId, [FromBody] UpdateCounterpartyCommand command)
    {
        command.CounterpartyId = counterpartyId;
        await _commandProcessor.ExecuteAsync<UpdateCounterpartyCommand, Empty>(command);
        return NoContent();
    }

    [HttpDelete("{counterpartyId:guid}")]
    public async Task<IActionResult> Delete(Guid counterpartyId)
    {
        await _commandProcessor.ExecuteAsync<DeleteCounterpartyCommand, Empty>(new DeleteCounterpartyCommand
        {
            CounterpartyId = counterpartyId
        });
        return NoContent();
    }
}
