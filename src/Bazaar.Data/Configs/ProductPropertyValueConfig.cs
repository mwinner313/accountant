using Bazaar.Core.Entities.ProductPropertyValue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class ProductPropertyValueConfig : IEntityTypeConfiguration<ProductPropertyValue>
{
    public void Configure(EntityTypeBuilder<ProductPropertyValue> builder)
    {
        builder.ToTable("ProductPropertyValues");
        builder.HasKey(p => p.ProductPropertyValueId);

        builder.Property(p => p.CreatedOn).HasPrecision(0);
        builder.Property(p => p.Value).HasMaxLength(500).IsRequired();
        builder.Property(p => p.ProductId).IsRequired();
        builder.Property(p => p.ProductPropertyId).IsRequired();
        builder.Property(p => p.Deleted).HasDefaultValue(false);
    }
}
