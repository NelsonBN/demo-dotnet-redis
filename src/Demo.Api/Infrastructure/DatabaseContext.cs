using Demo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Api.Infrastructure;

public class DatabaseContext : DbContext
{
    public DbSet<Product> Products { get; set; } = default!;

    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;

    public DatabaseContext(
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    {
        _loggerFactory = loggerFactory;
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseNpgsql(_configuration.GetConnectionString("Postgres")!)
            .UseLoggerFactory(_loggerFactory)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product
                .ToTable(nameof(Product))
                .HasKey(product => product.Id);

            product.Ignore(product => product.PutInCacheBy);
        });
    }
}
