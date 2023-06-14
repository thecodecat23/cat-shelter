namespace CatsShelter.Service.Features.Adoption.Infrastructure.Repositories.Exceptions;

public class CatUpdateException : Exception
{
    public CatUpdateException(string id) : base($"Update operation for cat with id {id} failed.")
    {
    }
}