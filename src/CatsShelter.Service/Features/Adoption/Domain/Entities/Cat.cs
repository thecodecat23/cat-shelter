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
            throw new InvalidOperationException("Cat is not available for adoption.");

        IsAvailable = false;
    }

    public void CancelAdoption() => IsAvailable = true;
}