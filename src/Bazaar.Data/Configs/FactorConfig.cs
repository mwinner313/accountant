using Bazaar.Core.Entities.Counterparty;
using Bazaar.Core.Entities.Factor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class FactorConfig : IEntityTypeConfiguration<Factor>
{
    public void Configure(EntityTypeBuilder<Factor> builder)
    {
        builder.ToTable("Factors");
        builder.HasKey(p => p.FactorId);
        builder.HasIndex(p => p.FactorId).IsUnique();
        builder.Ignore(p => p.UncommittedChanges);
        builder.Ignore(p => p.Version);

        builder.Property(p => p.Id).HasDefaultValueSql("NEWID()");
        builder.Property(p => p.CreateDate).HasPrecision(0).HasDefaultValueSql("(getdate())");

        builder.Property(p => p.ShopId).IsRequired();
        builder.Property(p => p.Type).IsRequired();
        builder.Property(p => p.CounterpartyId).IsRequired(false);
        builder.Property(p => p.Notes).HasMaxLength(1000).IsRequired(false);
        builder.Property(p => p.Date).HasPrecision(0).IsRequired();
        builder.Property(p => p.IsReversed).HasDefaultValue(false);
        builder.Property(p => p.Deleted).HasDefaultValue(false);

        builder.HasMany(f => f.Items)
            .WithOne()
            .HasForeignKey(i => i.FactorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Counterparty>()
            .WithMany()
            .HasForeignKey(f => f.CounterpartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.CounterpartyId);
    }
}
