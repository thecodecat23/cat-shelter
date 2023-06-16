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

    public CatsAdoptionGrpcServiceTests(GrpcTestFixture<Startup> factory)
    {
        _factory = factory;
        var channel = GrpcChannel.ForAddress(_factory.Server.BaseAddress, new GrpcChannelOptions { HttpClient = factory.CreateClient() });
        _client = new CatsShelterService.CatsShelterServiceClient(channel);
        _fixture = new Fixture();
    }
              
    [Fact]
    public async Task RequestAdoption_WithValidCatRequest_ReturnsSuccessfulAdoptionResponse()
    {
        // Arrange
        const string expectedMessage = "Adoption successful.";
        var cat = _fixture.Create<Service.Features.Adoption.Domain.Entities.Cat>();
        var catRequest = _fixture
            .Build<CatRequest>()
            .With(c => c.Id, cat.Id)
            .Create();

        await _factory.CatsCollection.InsertOneAsync(cat);

        // Act
        var response = await _client.RequestAdoptionAsync(catRequest);

        // Assert
        response.Should()
            .NotBeNull().And
            .BeEquivalentTo(new AdoptionResponse
            {
                Success = true,
                Message = expectedMessage
            });

        // Clean
        await _factory.CatsCollection.DeleteManyAsync(Builders<Service.Features.Adoption.Domain.Entities.Cat>.Filter.Empty);
    }
}