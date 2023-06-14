using AutoFixture;
using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Domain.Exceptions;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories.Exceptions;
using CatsShelter.Service.Features.Adoption.Services;
using FluentAssertions;
using Moq;

namespace CatsShelter.Service.UnitTests.Features.Adoption.Services;

public class CatsAdoptionServiceTests
{
    private readonly Mock<ICatsRepository> _mockRepository;
    private readonly ICatsAdoptionService _catsAdoptionService;
    private readonly Fixture _fixture;

    public CatsAdoptionServiceTests()
    {
        _mockRepository = new Mock<ICatsRepository>();
        _fixture = new Fixture();
        _catsAdoptionService = new CatsAdoptionService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAvailableCats_ShouldReturnMappedCats_WhenCatsAreAvailable()
    {
        // Arrange
        var cats = _fixture.Create<List<Cat>>();

        _mockRepository
            .Setup(r => r.GetAvailableCatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cats);

        // Act
        var result = await _catsAdoptionService.GetAvailableCatsAsync(default(CancellationToken));

        // Assert
        result.Should()
            .NotBeNull().And
            .BeEquivalentTo(cats);

        _mockRepository.Verify(r => r.GetAvailableCatsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnSuccessResponse_WhenAdoptionIsSuccessful()
    {
        // Arrange
        var catRequest = _fixture.Create<CatAdoptionRequest>();
        var cat = _fixture.Create<Cat>();
        var adoptionResponse = new SuccessRequestCatAdoptionResponse();

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        _mockRepository
            .Setup(r => r.UpdateCatAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _catsAdoptionService.RequestAdoptionAsync(catRequest, default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(adoptionResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.CatId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(cat, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnErrorResponse_WhenCatDoesNotExist()
    {
        // Arrange
        var catRequest = _fixture.Create<CatAdoptionRequest>();
        var expectedResponse = new FailCatAdoptionResponse(new CatNotFoundException(catRequest.CatId));

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new CatNotFoundException(catRequest.CatId));

        // Act
        var result = await _catsAdoptionService.RequestAdoptionAsync(catRequest, default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.CatId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnErrorResponse_WhenUpdateFails()
    {
        // Arrange
        var catRequest = _fixture.Create<CatAdoptionRequest>();
        var expectedResponse = new FailCatAdoptionResponse(new CatUpdateException(catRequest.CatId));

        var cat = _fixture.Create<Cat>();

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        _mockRepository
            .Setup(r => r.UpdateCatAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>()))
            .Throws(new CatUpdateException(catRequest.CatId));

        // Act
        var result = await _catsAdoptionService.RequestAdoptionAsync(catRequest, default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.CatId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnErrorResponse_WhenCatIsNotAvaiable()
    {
        // Arrange
        var catRequest = _fixture.Create<CatAdoptionRequest>();
        var expectedResponse = new FailCatAdoptionResponse(new CatUnavailableException());

        var cat = _fixture.Build<Cat>().Do(c => c.RequestAdoption()).Create();

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        // Act
        var result = await _catsAdoptionService.RequestAdoptionAsync(catRequest, default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.CatId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelAdoption_ShouldReturnSuccessResponse_WhenCancellationIsSuccessful()
    {
        // Arrange
        var catRequest = _fixture.Create<CatAdoptionRequest>();
        var cat = _fixture.Create<Cat>();
        var adoptionResponse = new SuccessCancelCatAdoptionResponse();

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        _mockRepository
            .Setup(r => r.UpdateCatAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _catsAdoptionService.CancelAdoptionAsync(catRequest, default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(adoptionResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.CatId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(cat, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAdoption_ShouldReturnErrorResponse_WhenCatDoesNotExist()
    {
        // Arrange
        var catRequest = _fixture.Create<CatAdoptionRequest>();
        var expectedResponse = new FailCatAdoptionResponse(new CatNotFoundException(catRequest.CatId));

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new CatNotFoundException(catRequest.CatId));

        // Act
        var result = await _catsAdoptionService.CancelAdoptionAsync(catRequest, default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.CatId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAdoption_ShouldReturnErrorResponse_WhenUpdateFails()
    {
        // Arrange
        var catRequest = _fixture.Create<CatAdoptionRequest>();
        var expectedResponse = new FailCatAdoptionResponse(new CatUpdateException(catRequest.CatId));

        var cat = _fixture.Create<Cat>();

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);
        _mockRepository
            .Setup(r => r.UpdateCatAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>()))
            .Throws(new CatUpdateException(catRequest.CatId));

        // Act
        var result = await _catsAdoptionService.CancelAdoptionAsync(catRequest, default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.CatId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}