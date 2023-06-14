using AutoFixture;
using AutoMapper;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories.Exceptions;
using CatsShelter.Service.Features.Adoption.Proto;
using CatsShelter.Service.Features.Adoption.Services;
using FluentAssertions;
using Moq;

namespace CatsShelter.Service.UnitTests.Features.Adoption.Services;

public class CatsAdoptionServiceTests
{
    private readonly Mock<ICatsRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ICatsAdoptionService _catsAdoptionService;
    private readonly Fixture _fixture;

    public CatsAdoptionServiceTests()
    {
        _mockRepository = new Mock<ICatsRepository>();
        _mockMapper = new Mock<IMapper>();
        _fixture = new Fixture();
        _catsAdoptionService = new CatsAdoptionService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAvailableCats_ShouldReturnMappedCats_WhenCatsAreAvailable()
    {
        // Arrange
        var domainCats = _fixture.Create<List<Service.Features.Adoption.Domain.Entities.Cat>>();
        var protoCats = domainCats.Select(cat => _mockMapper.Object.Map<Service.Features.Adoption.Proto.Cat>(cat)).ToList();

        _mockRepository
            .Setup(r => r.GetAvailableCatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainCats);

        foreach (var pair in domainCats.Zip(protoCats))
        {
            _mockMapper
                .Setup(m => m.Map<Service.Features.Adoption.Proto.Cat>(pair.First))
                .Returns(pair.Second);
        }

        // Act
        var result = await _catsAdoptionService.GetAvailableCats(new Empty());

        // Assert
        result.Cats_.Should().BeEquivalentTo(protoCats, options => options.ComparingByMembers<Service.Features.Adoption.Proto.Cat>());
        _mockRepository.Verify(r => r.GetAvailableCatsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnSuccessResponse_WhenAdoptionIsSuccessful()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();
        var domainCat = _fixture.Create<Service.Features.Adoption.Domain.Entities.Cat>();
        var protoCat = _mockMapper.Object.Map<Service.Features.Adoption.Proto.Cat>(domainCat);
        var adoptionResponse = new AdoptionResponse { Success = true, Message = "Adoption successful." };

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainCat);

        _mockMapper
            .Setup(m => m.Map<Service.Features.Adoption.Proto.Cat>(domainCat))
            .Returns(protoCat);

        _mockRepository
            .Setup(r => r.UpdateCatAsync(It.IsAny<Service.Features.Adoption.Domain.Entities.Cat>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _catsAdoptionService.RequestAdoption(catRequest);

        // Assert
        result.Should().BeEquivalentTo(adoptionResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(domainCat, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAdoption_ShouldReturnSuccessResponse_WhenCancellationIsSuccessful()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();
        var domainCat = _fixture.Create<Service.Features.Adoption.Domain.Entities.Cat>();
        var protoCat = _mockMapper.Object.Map<Service.Features.Adoption.Proto.Cat>(domainCat);
        var adoptionResponse = new AdoptionResponse { Success = true, Message = "Adoption cancelled." };

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainCat);

        _mockMapper
            .Setup(m => m.Map<Service.Features.Adoption.Proto.Cat>(domainCat))
            .Returns(protoCat);

        _mockRepository
            .Setup(r => r.UpdateCatAsync(It.IsAny<Service.Features.Adoption.Domain.Entities.Cat>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _catsAdoptionService.CancelAdoption(catRequest);

        // Assert
        result.Should().BeEquivalentTo(adoptionResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(domainCat, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnErrorResponse_WhenCatDoesNotExist()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();
        var expectedResponse = new AdoptionResponse
        {
            Success = false,
            Message = $"No cat with id {catRequest.Id} was found."
        };

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new CatNotFoundException(catRequest.Id));

        // Act
        var result = await _catsAdoptionService.RequestAdoption(catRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAdoption_ShouldReturnErrorResponse_WhenUpdateFails()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();
        var expectedResponse = new AdoptionResponse
        {
            Success = false,
            Message = $"Update operation for cat with id {catRequest.Id} failed."
        };

        var cat = _fixture.Create<Service.Features.Adoption.Domain.Entities.Cat>();
        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);
        _mockRepository
            .Setup(r => r.UpdateCatAsync(It.IsAny<Service.Features.Adoption.Domain.Entities.Cat>(), It.IsAny<CancellationToken>()))
            .Throws(new CatUpdateException(catRequest.Id));

        // Act
        var result = await _catsAdoptionService.RequestAdoption(catRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(It.IsAny<Service.Features.Adoption.Domain.Entities.Cat>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAdoption_ShouldReturnErrorResponse_WhenCatDoesNotExist()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();
        var expectedResponse = new AdoptionResponse
        {
            Success = false,
            Message = $"No cat with id {catRequest.Id} was found."
        };

        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new CatNotFoundException(catRequest.Id));

        // Act
        var result = await _catsAdoptionService.CancelAdoption(catRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAdoption_ShouldReturnErrorResponse_WhenUpdateFails()
    {
        // Arrange
        var catRequest = _fixture.Create<CatRequest>();
        var expectedResponse = new AdoptionResponse
        {
            Success = false,
            Message = $"Update operation for cat with id {catRequest.Id} failed."
        };

        var cat = _fixture.Create<Service.Features.Adoption.Domain.Entities.Cat>();
        _mockRepository
            .Setup(r => r.GetCatByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);
        _mockRepository
            .Setup(r => r.UpdateCatAsync(It.IsAny<Service.Features.Adoption.Domain.Entities.Cat>(), It.IsAny<CancellationToken>()))
            .Throws(new CatUpdateException(catRequest.Id));

        // Act
        var result = await _catsAdoptionService.CancelAdoption(catRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(r => r.GetCatByIdAsync(catRequest.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateCatAsync(It.IsAny<Service.Features.Adoption.Domain.Entities.Cat>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}