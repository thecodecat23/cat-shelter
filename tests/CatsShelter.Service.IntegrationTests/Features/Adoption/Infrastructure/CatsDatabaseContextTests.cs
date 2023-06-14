using Mongo2Go;
using MongoDB.Driver;
using AutoFixture;
using AutoFixture.Xunit2;
using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Infrastructure;
using FluentAssertions;

namespace CatsShelter.Service.IntegrationTests.Features.Adoption.Infrastructure;

public class CatsDatabaseContextTests
{
    private readonly IFixture _fixture;
    private readonly MongoDbRunner _runner;

    public CatsDatabaseContextTests()
    {
        _fixture = new Fixture();
        _runner = MongoDbRunner.Start();
    }

    [Theory, AutoData]
    public async Task FindCatAsync_ShouldReturnCat_WhenCatExists(Cat expectedCat)
    {
        // Arrange
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase("testDatabase");
        var collection = database.GetCollection<Cat>("testCollection");

        await collection.InsertOneAsync(expectedCat);

        var context = new CatsDatabaseContext(client, "testDatabase", "testCollection");

        // Act
        var cat = await context.FindCatAsync(expectedCat.Id, default);

        // Assert
        cat.Should().BeEquivalentTo(expectedCat);
    }

    [Theory, AutoData]
    public async Task ReplaceOneAsync_ShouldReturnAcknowledgedResult_WhenUpdateIsSuccessful(Cat cat)
    {
        // Arrange
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase("testDatabase");
        var collection = database.GetCollection<Cat>("testCollection");

        await collection.InsertOneAsync(cat);

        var context = new CatsDatabaseContext(client, "testDatabase", "testCollection");

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
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase("testDatabase");
        var collection = database.GetCollection<Cat>("testCollection");

        var availableCat = _fixture.Build<Cat>().Create();
        var unavailableCat = _fixture.Build<Cat>().Do(c => c.RequestAdoption()).Create();

        await collection.InsertManyAsync(new[] { availableCat, unavailableCat });

        var context = new CatsDatabaseContext(client, "testDatabase", "testCollection");

        // Act
        var cats = await context.GetAvailableCatsAsync(default);

        // Assert
        cats.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(availableCat);
    }
}