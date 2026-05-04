using Bazaar.Core.Entities.ProductProperty;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class ProductPropertyConfig : IEntityTypeConfiguration<ProductProperty>
{
    public void Configure(EntityTypeBuilder<ProductProperty> builder)
    {
        builder.ToTable("ProductProperties");
        builder.HasKey(p => p.ProductPropertyId);
        builder.HasIndex(p => p.ProductPropertyId).IsUnique();
        builder.Ignore(p => p.UncommittedChanges);
        builder.Ignore(p => p.Version);

        builder.Property(p => p.Id).HasDefaultValueSql("NEWID()");
        builder.Property(p => p.CreateDate).HasPrecision(0).HasDefaultValueSql("(getdate())");

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.ShopId).IsRequired();
        builder.Property(p => p.Deleted).HasDefaultValue(false);
    }
}
