using AutoFixture;
using AutoFixture.Xunit2;
using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Infrastructure;
using FluentAssertions;
using Mongo2Go;
using MongoDB.Driver;

namespace CatsShelter.Service.IntegrationTests.Features.Adoption.Infrastructure;

public class CatsDatabaseContextTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly MongoDbRunner _runner;

    public CatsDatabaseContextTests()
    {
        _fixture = new Fixture();
        _runner = MongoDbRunner.Start();
    }

    public void Dispose()
    {
        _runner.Dispose();
    }

    [Theory, AutoData]
    public async Task FindCatAsync_ShouldReturnCat_WhenCatExists(Cat expectedCat)
    {
        // Arrange
        var databaseName = $"TestDatabase_{Guid.NewGuid()}";
        var collectionName = $"TestCollection_{Guid.NewGuid()}";
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<Cat>(collectionName);

        await collection.InsertOneAsync(expectedCat);

        var context = new CatsDatabaseContext(client, databaseName, collectionName);

        // Act
        var cat = await context.FindCatAsync(expectedCat.Id, default);

        // Assert
        cat.Should().BeEquivalentTo(expectedCat);
    }

    [Fact]
    public async Task FindCatAsync_ShouldReturnNull_WhenDoesNotExists()
    {
        // Arrange
        var databaseName = $"TestDatabase_{Guid.NewGuid()}";
        var collectionName = $"TestCollection_{Guid.NewGuid()}";
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase(databaseName);
        database.GetCollection<Cat>(collectionName);

        var context = new CatsDatabaseContext(client, databaseName, collectionName);

        // Act
        var cat = await context.FindCatAsync(_fixture.Create<Guid>().ToString(), default);

        // Assert
        cat.Should().BeNull();
    }

    [Theory, AutoData]
    public async Task ReplaceOneAsync_ShouldReturnAcknowledgedResult_WhenUpdateIsSuccessful(Cat cat)
    {
        // Arrange
        var databaseName = $"TestDatabase_{Guid.NewGuid()}";
        var collectionName = $"TestCollection_{Guid.NewGuid()}";
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<Cat>(collectionName);

        await collection.InsertOneAsync(cat);

        var context = new CatsDatabaseContext(client, databaseName, collectionName);

        cat.RequestAdoption();

        // Act
        var result = await context.ReplaceOneAsync(cat, default);

        // Assert
        result.Should().BeEquivalentTo(new CatUpdateResult
        {
            IsAcknowledged = true,
            ModifiedCount = 1
        });
    }

    [Fact]
    public async Task GetAvailableCatsAsync_ShouldReturnAvailableCats_WhenThereAreAvailableCats()
    {
        // Arrange
        var databaseName = $"TestDatabase_{Guid.NewGuid()}";
        var collectionName = $"TestCollection_{Guid.NewGuid()}";
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<Cat>(collectionName);

        var availableCat = _fixture.Build<Cat>().Create();
        var unavailableCat = _fixture.Build<Cat>().Do(c => c.RequestAdoption()).Create();

        await collection.InsertManyAsync(new[] { availableCat, unavailableCat });

        var context = new CatsDatabaseContext(client, databaseName, collectionName);

        // Act
        var cats = await context.GetAvailableCatsAsync(default);

        // Assert
        cats.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(availableCat);
    }

    [Fact]
    public async Task SeedDatabase_ShouldSeedCats_WhenCalled()
    {
        // Arrange
        var databaseName = $"TestDatabase_{Guid.NewGuid()}";
        var collectionName = $"TestCollection_{Guid.NewGuid()}";
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<Cat>(collectionName);
        int expectedCats = 10;

        var context = new CatsDatabaseContext(client, databaseName, collectionName);

        // Act
        await context.SeedDatabase(expectedCats, CancellationToken.None);

        // Assert
        var actualCats = await collection.Find(FilterDefinition<Cat>.Empty).ToListAsync();
        actualCats.Count.Should().Be(expectedCats);
    }
}