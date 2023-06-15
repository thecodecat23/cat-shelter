using AutoFixture;
using CatsShelter.Service.Features.Adoption.Proto;
using FluentAssertions;
using Grpc.Net.Client;

namespace CatsShelter.Service.IntegrationTests.Features.Adoption.Services;

public class CatsAdoptionGrpcServiceTests : IClassFixture<GrpcTestFixture<Startup>>
{
    private readonly CatsShelterService.CatsShelterServiceClient _client;
    private readonly Fixture _fixture;

    public CatsAdoptionGrpcServiceTests(GrpcTestFixture<Startup> factory)
    {
        var channel = GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions { HttpClient = factory.CreateClient() });
        _client = new CatsShelterService.CatsShelterServiceClient(channel);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task ShouldRequestAdoptionSuccessfully()
    {
        // Arrange
        const string expectedMessage = "Adoption successful.";
        var catRequest = _fixture.Create<CatRequest>();

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
    }
}