namespace Demo.Api.Models;

public record ProductResponse(
    uint Id,
    string Name,
    double Price,
    int Quantity,
    string From,
    string? PutInCacheBy,
    string ReturnedBy);