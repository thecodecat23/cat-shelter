using CatsShelter.Service.Features.Adoption.Proto;
using CatsShelter.Service.Features.Adoption.Services;
using Grpc.Core;
using Moq;

namespace CatsShelter.Service.UnitTests.Features.Adoption.Services;

public class CatsAdoptionGrpcServiceTests
{
    private Mock<ICatsAdoptionService> _mockCatsAdoptionService;
    private CatsAdoptionGrpcService _catsAdoptionGrpcService;
    private Mock<ServerCallContext> _mockServerCallContext;

    public CatsAdoptionGrpcServiceTests()
    {
        _mockCatsAdoptionService = new Mock<ICatsAdoptionService>();
        _catsAdoptionGrpcService = new CatsAdoptionGrpcService(_mockCatsAdoptionService.Object);
        _mockServerCallContext = new Mock<ServerCallContext>();
    }

    [Fact]
    public async Task GetAvailableCats_ShouldCallServiceMethod()
    {
        // Arrange
        var request = new Empty();

        // Act
        await _catsAdoptionGrpcService.GetAvailableCats(request, _mockServerCallContext.Object);

        // Assert
        _mockCatsAdoptionService.Verify(service => service.GetAvailableCats(request), Times.Once);
    }

    [Fact]
    public async Task RequestAdoption_ShouldCallServiceMethod()
    {
        // Arrange
        var request = new CatRequest();

        // Act
        await _catsAdoptionGrpcService.RequestAdoption(request, _mockServerCallContext.Object);

        // Assert
        _mockCatsAdoptionService.Verify(service => service.RequestAdoption(request), Times.Once);
    }

    [Fact]
    public async Task CancelAdoption_ShouldCallServiceMethod()
    {
        // Arrange
        var request = new CatRequest();

        // Act
        await _catsAdoptionGrpcService.CancelAdoption(request, _mockServerCallContext.Object);

        // Assert
        _mockCatsAdoptionService.Verify(service => service.CancelAdoption(request), Times.Once);
    }
}