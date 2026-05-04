using Bazaar.Core.Entities.Factor;
using Bazaar.Core.Entities.Factor.Events;
using Infra;
using Infra.Commands;
using Infra.EFCore;

namespace Bazaar.Handlers.Commands.Factors.Create;

public class CreateFactorCommandHandler : ICommandHandler<CreateFactorCommand, CreateFactorCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFactorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateFactorCommandResult> HandleAsync(CreateFactorCommand command, CancellationToken cancellationToken)
    {
        var items = command.Items
            .Select(i => new FactorItemData(i.ProductId, i.Amount, i.UnitPrice))
            .ToList();

        var factor = new Factor(command.ShopId, command.Type, command.Notes, command.Date, items);

        _unitOfWork.Repo<Factor>().Add(factor);
        await _unitOfWork.Save(factor);

        return new CreateFactorCommandResult { FactorId = factor.Id };
    }
}
