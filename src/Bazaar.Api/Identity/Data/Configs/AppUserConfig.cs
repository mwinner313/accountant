using Bazaar.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bazaar.Identity.Data.Configs;

public class AppUserConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(u => u.PhoneNumber).IsUnique();
        builder.Property(u => u.IsActive).HasDefaultValue(true);
    }
}
