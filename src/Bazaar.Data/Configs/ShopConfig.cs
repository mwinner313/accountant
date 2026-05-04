using Bazaar.Core.Entities.Shop;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class ShopConfig : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.ToTable("Shops");
        builder.HasKey(p => p.ShopId);
        builder.HasIndex(p => p.ShopId).IsUnique();
        builder.Ignore(p => p.UncommittedChanges);
        builder.Ignore(p => p.Version);

        builder.Property(p => p.Id).HasDefaultValueSql("NEWID()");
        builder.Property(p => p.CreateDate).HasPrecision(0).HasDefaultValueSql("(getdate())");

        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.OwnerId).IsRequired();
        builder.Property(p => p.Deleted).HasDefaultValue(false);
    }
}
