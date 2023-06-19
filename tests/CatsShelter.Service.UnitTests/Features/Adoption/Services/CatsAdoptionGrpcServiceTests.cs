using AutoFixture;
using AutoMapper;
using CatsShelter.Service.Features.Adoption.Proto;
using CatsShelter.Service.Features.Adoption.Services;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Testing;
using Grpc.Core.Utils;
using Moq;

namespace CatsShelter.Service.UnitTests.Features.Adoption.Services;

public class CatsAdoptionGrpcServiceTests
{
    private readonly Mock<ICatsAdoptionService> _mockCatsAdoptionService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Fixture _fixture;
    private readonly CatsAdoptionGrpcService _service;
    private readonly ServerCallContext _mockContext;

    public CatsAdoptionGrpcServiceTests()
    {
        _mockCatsAdoptionService = new Mock<ICatsAdoptionService>();
        _mockMapper = new Mock<IMapper>();
        _fixture = new Fixture();
        _service = new CatsAdoptionGrpcService(_mockCatsAdoptionService.Object, _mockMapper.Object);

        var tokenSource = new CancellationTokenSource();
        _mockContext = TestServerCallContext.Create(
            "mockMethod",
            null,
            DateTime.UtcNow.AddHours(1),
            new Metadata(),
            tokenSource.Token,
            "mockPeer",
            null,
            null,
            m => TaskUtils.CompletedTask,
            () => new WriteOptions(),
            writeOptions => { }
        );
    }

    [Fact]
    public async Task RequestAdoption_Should_Call_CatsAdoptionService_And_Return_AdoptionResponse()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();
        var catAdoptionRequest = _fixture.Create<CatAdoptionRequest>();
        var catAdoptionResponse = _fixture.Create<CatAdoptionResponse>();
        var adoptionResponse = _fixture.Create<AdoptionResponse>();

        _mockMapper.Setup(m => m.Map<CatAdoptionRequest>(catRequest)).Returns(catAdoptionRequest);
        _mockCatsAdoptionService.Setup(s => s.RequestAdoptionAsync(catAdoptionRequest, It.IsAny<CancellationToken>())).ReturnsAsync(catAdoptionResponse);
        _mockMapper.Setup(m => m.Map<AdoptionResponse>(catAdoptionResponse)).Returns(adoptionResponse);

        // Act
        var response = await _service.RequestAdoption(catRequest, _mockContext);

        // Assert
        _mockCatsAdoptionService.Verify(s => s.RequestAdoptionAsync(catAdoptionRequest, It.IsAny<CancellationToken>()), Times.Once);
        response.Should().BeEquivalentTo(adoptionResponse);
    }

    [Fact]
    public async Task GetAvailableCats_Should_Call_CatsAdoptionService_And_Return_Cats()
    {
        // Arrange
        var domainCats = _fixture.Create<List<Service.Features.Adoption.Domain.Entities.Cat>>();
        var protoCats = _fixture.Create<List<Service.Features.Adoption.Proto.Cat>>();
        var expectedCats = new Cats { Cats_ = { protoCats } };

        _mockCatsAdoptionService.Setup(s => s.GetAvailableCatsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(domainCats);
        _mockMapper.Setup(m => m.Map<List<Service.Features.Adoption.Proto.Cat>>(domainCats)).Returns(protoCats);

        // Act
        var response = await _service.GetAvailableCats(new Empty(), _mockContext);

        // Assert
        _mockCatsAdoptionService.Verify(s => s.GetAvailableCatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        response.Should().BeEquivalentTo(expectedCats);
    }

    [Fact]
    public async Task CancelAdoption_Should_Call_CatsAdoptionService_And_Return_AdoptionResponse()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();
        var catAdoptionRequest = _fixture.Create<CatAdoptionRequest>();
        var catAdoptionResponse = _fixture.Create<CatAdoptionResponse>();
        var adoptionResponse = _fixture.Create<AdoptionResponse>();

        _mockMapper.Setup(m => m.Map<CatAdoptionRequest>(catRequest)).Returns(catAdoptionRequest);
        _mockCatsAdoptionService.Setup(s => s.CancelAdoptionAsync(catAdoptionRequest, It.IsAny<CancellationToken>())).ReturnsAsync(catAdoptionResponse);
        _mockMapper.Setup(m => m.Map<AdoptionResponse>(catAdoptionResponse)).Returns(adoptionResponse);

        // Act
        var response = await _service.CancelAdoption(catRequest, _mockContext);

        // Assert
        _mockCatsAdoptionService.Verify(s => s.CancelAdoptionAsync(catAdoptionRequest, It.IsAny<CancellationToken>()), Times.Once);
        response.Should().BeEquivalentTo(adoptionResponse);
    }
}