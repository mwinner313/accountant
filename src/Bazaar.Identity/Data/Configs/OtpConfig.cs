using Bazaar.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Identity.Data.Configs;

public class OtpConfig : IEntityTypeConfiguration<Otp>
{
    public void Configure(EntityTypeBuilder<Otp> builder)
    {
        builder.ToTable("Otps");
        builder.HasKey(o => o.OtpId);
        builder.Property(o => o.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(o => o.Code).HasMaxLength(10).IsRequired();
        builder.Property(o => o.ExpiresAt).HasPrecision(0);
        builder.Property(o => o.IsUsed).HasDefaultValue(false);
        builder.Property(o => o.CreatedOn).HasPrecision(0).HasDefaultValueSql("(getdate())");
    }
}
