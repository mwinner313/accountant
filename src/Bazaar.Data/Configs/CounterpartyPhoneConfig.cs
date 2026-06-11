using Bazaar.Core.Entities.Counterparty;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class CounterpartyPhoneConfig : IEntityTypeConfiguration<CounterpartyPhone>
{
    public void Configure(EntityTypeBuilder<CounterpartyPhone> builder)
    {
        builder.ToTable("CounterpartyPhones");
        builder.HasKey(p => p.CounterpartyPhoneId);

        builder.Property(p => p.CounterpartyId).IsRequired();
        builder.Property(p => p.Number).HasMaxLength(40).IsRequired();
    }
}
