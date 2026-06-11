using Bazaar.Core.Entities.Counterparty;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class CounterpartyConfig : IEntityTypeConfiguration<Counterparty>
{
    public void Configure(EntityTypeBuilder<Counterparty> builder)
    {
        builder.ToTable("Counterparties");
        builder.HasKey(p => p.CounterpartyId);
        builder.HasIndex(p => p.CounterpartyId).IsUnique();
        builder.Ignore(p => p.UncommittedChanges);
        builder.Ignore(p => p.Version);

        builder.Property(p => p.Id).HasDefaultValueSql("NEWID()");
        builder.Property(p => p.CreateDate).HasPrecision(0).HasDefaultValueSql("(getdate())");

        builder.Property(p => p.OwnerId).IsRequired();
        builder.Property(p => p.FullName).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Deleted).HasDefaultValue(false);

        builder.HasMany(c => c.Phones)
            .WithOne()
            .HasForeignKey(p => p.CounterpartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.BankAccounts)
            .WithOne()
            .HasForeignKey(b => b.CounterpartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => new { p.OwnerId, p.Deleted });
        builder.HasIndex(p => p.FullName);
    }
}
