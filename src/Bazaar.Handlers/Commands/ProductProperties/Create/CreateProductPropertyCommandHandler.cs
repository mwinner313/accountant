using Bazaar.Core.Entities.ProductProperty;
using Infra;
using Infra.Commands;
using Infra.EFCore;

namespace Bazaar.Handlers.Commands.ProductProperties.Create;

public class CreateProductPropertyCommandHandler : ICommandHandler<CreateProductPropertyCommand, CreateProductPropertyCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductPropertyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateProductPropertyCommandResult> HandleAsync(CreateProductPropertyCommand command, CancellationToken cancellationToken)
    {
        var property = new ProductProperty(command.ShopId, command.Name);
        _unitOfWork.Repo<ProductProperty>().Add(property);
        await _unitOfWork.Save(property);

        return new CreateProductPropertyCommandResult { ProductPropertyId = property.Id, Name = property.Name };
    }
}
