using Bazaar.Handlers;
using Bazaar.Handlers.Commands.Categories.Create;
using Bazaar.Handlers.Commands.Categories.Delete;
using Bazaar.Handlers.Commands.Categories.Update;
using Bazaar.Handlers.Queries.Categories.GetShopCategories;
using Infra.Commands;
using Infra.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bazaar.Api.Controllers;

[ApiController]
[Route("api/shops/{shopId:guid}/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    public CategoriesController(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid shopId)
    {
        var result = await _queryProcessor.ExecuteAsync(new GetShopCategoriesQuery { ShopId = shopId });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid shopId, [FromBody] CreateCategoryCommand command)
    {
        command.ShopId = shopId;
        var result = await _commandProcessor.ExecuteAsync<CreateCategoryCommand, CreateCategoryCommandResult>(command);
        return Ok(result);
    }

    [HttpPut("{categoryId:guid}")]
    public async Task<IActionResult> Update(Guid categoryId, [FromBody] UpdateCategoryCommand command)
    {
        command.CategoryId = categoryId;
        await _commandProcessor.ExecuteAsync<UpdateCategoryCommand, Empty>(command);
        return NoContent();
    }

    [HttpDelete("{categoryId:guid}")]
    public async Task<IActionResult> Delete(Guid categoryId)
    {
        await _commandProcessor.ExecuteAsync<DeleteCategoryCommand, Empty>(new DeleteCategoryCommand { CategoryId = categoryId });
        return NoContent();
    }
}
