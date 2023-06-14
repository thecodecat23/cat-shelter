namespace CatsShelter.Service.Features.Adoption.Infrastructure;

public class CatUpdateResult
{
    public bool IsAcknowledged { get; set; }
    public long ModifiedCount { get; set; }
}