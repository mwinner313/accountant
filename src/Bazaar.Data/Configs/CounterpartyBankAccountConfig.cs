using Bazaar.Core.Entities.Counterparty;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Data.Configs;

public class CounterpartyBankAccountConfig : IEntityTypeConfiguration<CounterpartyBankAccount>
{
    public void Configure(EntityTypeBuilder<CounterpartyBankAccount> builder)
    {
        builder.ToTable("CounterpartyBankAccounts");
        builder.HasKey(p => p.CounterpartyBankAccountId);

        builder.Property(p => p.CounterpartyId).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.AccountNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ShebaNumber).HasMaxLength(34).IsRequired();
        builder.Property(p => p.CardNumber).HasMaxLength(20).IsRequired();
    }
}
