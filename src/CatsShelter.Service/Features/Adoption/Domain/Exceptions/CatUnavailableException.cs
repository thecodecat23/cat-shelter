namespace CatsShelter.Service.Features.Adoption.Domain.Exceptions;

public class CatUnavailableException : Exception
{
    public CatUnavailableException()
        : base("Cat is not available for adoption.")
    {
    }
}