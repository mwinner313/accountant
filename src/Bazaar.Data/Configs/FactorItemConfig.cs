using Bazaar.Core.Entities.Factor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class FactorItemConfig : IEntityTypeConfiguration<FactorItem>
{
    public void Configure(EntityTypeBuilder<FactorItem> builder)
    {
        builder.ToTable("FactorItems");
        builder.HasKey(p => p.FactorItemId);

        builder.Property(p => p.FactorId).IsRequired();
        builder.Property(p => p.ProductId).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(18, 4).IsRequired();
        builder.Property(p => p.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.CreatedOn).HasPrecision(0).HasDefaultValueSql("(getdate())");
    }
}
