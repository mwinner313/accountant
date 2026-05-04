namespace Bazaar.Core.Entities.ProductPropertyValue;

public class ProductPropertyValue
{
    public int ProductPropertyValueId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductPropertyId { get; set; }
    public string Value { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
    public bool Deleted { get; set; }
}
