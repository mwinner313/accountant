using Bazaar.Core.Entities.Product;
using Infra;
using Infra.Commands;
using Infra.EFCore;

namespace Bazaar.Handlers.Commands.Products.Create;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateProductCommandResult> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product(
            command.ShopId,
            command.CategoryId,
            command.Name,
            command.Unit,
            command.Picture,
            command.SellPrice,
            command.BuyPrice);

        _unitOfWork.Repo<Product>().Add(product);
        await _unitOfWork.Save(product);

        return new CreateProductCommandResult { ProductId = product.Id, Name = product.Name };
    }
}
