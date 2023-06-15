namespace CatsShelter.Service.Features.Adoption.Infrastructure;

public interface ICatsDatabaseContextFactory
{
    ICatsDatabaseContext Create(string databaseName, string collectionName);
}