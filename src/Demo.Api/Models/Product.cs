namespace Demo.Api.Models;

public class Product
{
    public uint Id { get; set; }
    public required string Name { get; set; } = default!;
    public required double Price { get; set; }
    public required int Quantity { get; set; }

    public string? PutInCacheBy { get; set; }

    public static Product Create(string name, double price, int quantity)
        => new()
        {
            Name = name,
            Price = price,
            Quantity = quantity
        };
}
