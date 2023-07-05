using Demo.Api.Infrastructure;
using Demo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Demo.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private static readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
        SlidingExpiration = TimeSpan.FromSeconds(10)
    };

    private readonly IDistributedCache _cache;
    private readonly DatabaseContext _db;

    public ProductsController(
        IDistributedCache cache,
        DatabaseContext db)
    {
        _cache = cache;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _db
            .Products
            .Select(product => new ProductResponse(
                product.Id,
                product.Name,
                product.Price,
                product.Quantity,
                "Database",
                null,
                Environment.MachineName))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id}", Name = nameof(GetProduct))]
    public async Task<IActionResult> GetProduct(uint id, CancellationToken cancellationToken)
    {
        var key = $"{typeof(Product)}:{id}";
        var product = await _cache.GetAsync<Product>(key, cancellationToken);

        string from = "Cache";
        string? putInCacheBy = product?.PutInCacheBy;

        if(product is null)
        {
            product = await _db.Products.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (product is null)
            {
                return NotFound();
            }

            from = "Database";
            product.PutInCacheBy = Environment.MachineName;

            await _cache.SetAsync(key, product, _cacheOptions, cancellationToken);
        }

        return Ok(new ProductResponse(
            product.Id,
            product.Name,
            product.Price,
            product.Quantity,
            from,
            putInCacheBy,
            Environment.MachineName));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.Name,
            request.Price,
            request.Quantity);

        _db.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        product.PutInCacheBy = Environment.MachineName;

        var key = $"{typeof(Product)}:{product.Id}";
        await _cache.SetAsync(key, product, _cacheOptions, cancellationToken);

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id.ToString() },
            new ProductResponse(
                product.Id,
                product.Name,
                product.Price,
                product.Quantity,
                "Created",
                null,
                Environment.MachineName));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(uint id, [FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = request.Name;
        product.Price = request.Price;

        _db.Update(product);
        await _db.SaveChangesAsync(cancellationToken);

        product.PutInCacheBy = Environment.MachineName;

        var key = $"{typeof(Product)}:{product.Id}";
        await _cache.SetAsync(key, product, _cacheOptions, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(uint id, CancellationToken cancellationToken)
    {
        if (await _db.Products.AnyAsync(a => a.Id == id, cancellationToken))
        {
            await _db.Products
                .Where(w => w.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var key = $"{typeof(Product)}:{id}";
            await _cache.RemoveAsync(key, cancellationToken);

            return NoContent();
        }

        return NotFound();
    }
}
