using Bazaar.Core.Entities.Category;
using Bazaar.Core.Entities.Counterparty;
using Bazaar.Core.Entities.Factor;
using Bazaar.Core.Entities.Product;
using Bazaar.Core.Entities.ProductProperty;
using Bazaar.Core.Entities.ProductPropertyValue;
using Bazaar.Core.Entities.Shop;
using Bazaar.Data.Configs;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Data;

public class BazaarDbContext : DbContext
{
    public BazaarDbContext(DbContextOptions<BazaarDbContext> options) : base(options) { }

    public DbSet<Shop> Shops { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductProperty> ProductProperties { get; set; }
    public DbSet<ProductPropertyValue> ProductPropertyValues { get; set; }
    public DbSet<Counterparty> Counterparties { get; set; }
    public DbSet<CounterpartyPhone> CounterpartyPhones { get; set; }
    public DbSet<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; }
    public DbSet<Factor> Factors { get; set; }
    public DbSet<FactorItem> FactorItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopConfig).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
