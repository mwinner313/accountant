using Bazaar.Core.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.ProductId);
        builder.HasIndex(p => p.ProductId).IsUnique();
        builder.Ignore(p => p.UncommittedChanges);
        builder.Ignore(p => p.Version);

        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Unit).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Picture).HasMaxLength(500).IsRequired(false);
        builder.Property(p => p.SellPrice).HasPrecision(18, 2);
        builder.Property(p => p.BuyPrice).HasPrecision(18, 2);
        builder.Property(p => p.InventoryAmount).HasPrecision(18, 4).HasDefaultValue(0m);
        builder.Property(p => p.ShopId).IsRequired();
        builder.Property(p => p.CategoryId).IsRequired(false);
        builder.Property(p => p.Deleted).HasDefaultValue(false);
    }
}
