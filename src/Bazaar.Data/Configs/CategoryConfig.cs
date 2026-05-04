using Bazaar.Core.Entities.Category;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(p => p.CategoryId);
        builder.HasIndex(p => p.CategoryId).IsUnique();
        builder.Ignore(p => p.UncommittedChanges);
        builder.Ignore(p => p.Version);

        builder.Property(p => p.Id).HasDefaultValueSql("NEWID()");
        builder.Property(p => p.CreateDate).HasPrecision(0).HasDefaultValueSql("(getdate())");

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.ShopId).IsRequired();
        builder.Property(p => p.Deleted).HasDefaultValue(false);
    }
}
