using AutoFixture;
using CatsShelter.Service.Features.Adoption.Proto;
using FluentAssertions;
using Grpc.Net.Client;
using MongoDB.Driver;

namespace CatsShelter.Service.IntegrationTests.Features.Adoption.Services;

public class CatsAdoptionGrpcServiceTests : IClassFixture<GrpcTestFixture<Startup>>
{
    private readonly CatsShelterService.CatsShelterServiceClient _client;
    private readonly Fixture _fixture;
    private readonly GrpcTestFixture<Startup> _factory;

    private const string ExpectedAdoptionSuccessMessage = "Adoption successful.";
    private const string ExpectedCancelAdoptionSuccessMessage = "Adoption canceled.";

    public CatsAdoptionGrpcServiceTests(GrpcTestFixture<Startup> factory)
    {
        _factory = factory;
        var channel = GrpcChannel.ForAddress(_factory.Server.BaseAddress, new GrpcChannelOptions { HttpClient = factory.CreateClient() });
        _client = new CatsShelterService.CatsShelterServiceClient(channel);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetAvailableCats_ShouldReturnMappedCats_WhenCatsAreAvailable()
    {
        // Arrange
        var cats = _fixture.CreateMany<Service.Features.Adoption.Domain.Entities.Cat>(5);

        await _factory.CatsCollection!.InsertManyAsync(cats);

        // Act
        var response = await _client.GetAvailableCatsAsync(new Empty());

        // Assert
        response.Should()
            .NotBeNull().And
            .BeEquivalentTo(new Cats
            {
                Cats_ = 
                { 
                    cats.Select(c => new Cat
                    {
                        Id = c.Id,
                        Name = c.Name,
                        IsAvailable = c.IsAvailable,
                    })
                }
            });

        // Clean
        await _factory.CatsCollection.DeleteManyAsync(Builders<Service.Features.Adoption.Domain.Entities.Cat>.Filter.Empty);
    }

    [Fact]
    public async Task GetAvailableCats_ShouldReturnEmptyList_WhenNoCatsAreAvailable()
    {
        // Act
        var response = await _client.GetAvailableCatsAsync(new Empty());

        // Assert
        response.Should()
            .NotBeNull().And
            .BeEquivalentTo(new Cats
            {
                Cats_ = { }
            });
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnSuccessResponse_WhenAdoptionIsSuccessful()
    {
        // Arrange
        var cat = _fixture.Create<Service.Features.Adoption.Domain.Entities.Cat>();

        var catRequest = _fixture
            .Build<CatRequest>()
            .With(c => c.Id, cat.Id)
            .Create();

        await _factory.CatsCollection!.InsertOneAsync(cat);

        // Act
        var response = await _client.RequestAdoptionAsync(catRequest);

        // Assert
        response.Should()
            .NotBeNull().And
            .BeEquivalentTo(new AdoptionResponse
            {
                Success = true,
                Message = ExpectedAdoptionSuccessMessage
            });


        // Clean
        await _factory.CatsCollection.DeleteManyAsync(Builders<Service.Features.Adoption.Domain.Entities.Cat>.Filter.Empty);
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnErrorResponse_WhenCatDoesNotExist()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();

        // Act
        var response = await _client.RequestAdoptionAsync(catRequest);

        // Assert
        response.Should()
            .NotBeNull().And
            .BeEquivalentTo(new AdoptionResponse
            {
                Success = false,
                Message = $"No cat with id {catRequest.Id} was found."
            });
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnErrorResponse_WhenCatIsNotAvaiable()
    {
        // Arrange
        var cat = _fixture
            .Build<Service.Features.Adoption.Domain.Entities.Cat>()
            .Do(c => c.RequestAdoption())
            .Create();

        var catRequest = _fixture
            .Build<CatRequest>()
            .With(c => c.Id, cat.Id)
            .Create();

        await _factory.CatsCollection!.InsertOneAsync(cat);

        // Act
        var response = await _client.RequestAdoptionAsync(catRequest);

        // Assert
        response.Should()
            .NotBeNull().And
            .BeEquivalentTo(new AdoptionResponse
            {
                Success = false,
                Message = "Cat is not available for adoption."
            });

        // Clean
        await _factory.CatsCollection.DeleteManyAsync(Builders<Service.Features.Adoption.Domain.Entities.Cat>.Filter.Empty);
    }

    [Fact]
    public async Task RequestAdoption_MultipleClients_ShouldAllowOnlyOneAdoption_WhenMultipleClientsTryToAdoptSameCat()
    {
        // Arrange
        var cat = _fixture.Create<Service.Features.Adoption.Domain.Entities.Cat>();
      
        var catRequest = _fixture
            .Build<CatRequest>()
            .With(c => c.Id, cat.Id)
            .Create();

        await _factory.CatsCollection!.InsertOneAsync(cat);

        // Act
        var tasks = new List<Task<AdoptionResponse>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.RequestAdoptionAsync(catRequest).ResponseAsync);
        }
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Count(r => r.Success).Should().Be(1);
        responses.Count(r => !r.Success).Should().Be(9);

        // Cleanup
        await _factory.CatsCollection.DeleteManyAsync(Builders<Service.Features.Adoption.Domain.Entities.Cat>.Filter.Empty);
    }


    [Fact]
    public async Task CancelAdoption_ShouldReturnSuccessResponse_WhenCancellationIsSuccessful()
    {
        // Arrange
        var cat = _fixture
            .Build<Service.Features.Adoption.Domain.Entities.Cat>()
            .Do(c => c.RequestAdoption())
            .Create();

        var catRequest = _fixture
            .Build<CatRequest>()
            .With(c => c.Id, cat.Id)
            .Create();

        await _factory.CatsCollection!.InsertOneAsync(cat);

        // Act
        var response = await _client.CancelAdoptionAsync(catRequest);

        // Assert
        response.Should()
            .NotBeNull().And
            .BeEquivalentTo(new AdoptionResponse
            {
                Success = true,
                Message = ExpectedCancelAdoptionSuccessMessage
            });

        // Clean
        await _factory.CatsCollection.DeleteManyAsync(Builders<Service.Features.Adoption.Domain.Entities.Cat>.Filter.Empty);
    }

    [Fact]
    public async Task CancelAdoption_ShouldReturnErrorResponse_WhenCatDoesNotExist()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();

        // Act
        var response = await _client.CancelAdoptionAsync(catRequest);

        // Assert
        response.Should()
            .NotBeNull().And
            .BeEquivalentTo(new AdoptionResponse
            {
                Success = false,
                Message = $"No cat with id {catRequest.Id} was found."
            });
    }
}