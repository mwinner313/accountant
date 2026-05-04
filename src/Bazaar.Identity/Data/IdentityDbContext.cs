using Bazaar.Identity.Data.Configs;
using Bazaar.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Identity.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Otp> Otps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppUserConfig).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
