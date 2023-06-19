using CatsShelter.Service.Features.Adoption.Domain.Exceptions;

namespace CatsShelter.Service.Features.Adoption.Domain.Entities;

public class Cat
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public bool IsAvailable { get; private set; }

    public Cat(string id, string name)
    {
        Id = id;
        Name = name;
        IsAvailable = true;
    }

    public void RequestAdoption()
    {
        if (!IsAvailable)
            throw new CatUnavailableException();

        IsAvailable = false;
    }

    public void CancelAdoption() => IsAvailable = true;
}