using AutoFixture;
using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Infrastructure;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories.Exceptions;
using FluentAssertions;
using Moq;

namespace CatsShelter.Service.UnitTests.Features.Adoption.Infrastructure.Repositories;

public class CatsRepositoryTests
{
    private readonly Mock<ICatsDatabaseContext> _mockContext;
    private readonly CatsRepository _catsRepository;
    private readonly Fixture _fixture;

    public CatsRepositoryTests()
    {
        _mockContext = new Mock<ICatsDatabaseContext>();
        _catsRepository = new CatsRepository(_mockContext.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetCatByIdAsync_ShouldReturnCat_WhenCatExists()
    {
        // Arrange
        var cat = _fixture.Create<Cat>();
        
        _mockContext
            .Setup(c => c.FindCatAsync(cat.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        // Act
        var result = await _catsRepository.GetCatByIdAsync(cat.Id, default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(cat);
        _mockContext.Verify(c => c.FindCatAsync(cat.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCatByIdAsync_ShouldThrowCatNotFoundException_WhenCatDoesNotExist()
    {
        // Arrange
        var catId = _fixture.Create<string>();
        
        _mockContext
            .Setup(c => c.FindCatAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cat)null!);

        // Act & Assert
        Func<Task> act = async () => await _catsRepository.GetCatByIdAsync(catId, default(CancellationToken));
        await act.Should().ThrowAsync<CatNotFoundException>();
        _mockContext.Verify(c => c.FindCatAsync(catId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCatAsync_ShouldNotThrow_WhenUpdateIsSuccessful()
    {
        // Arrange
        var cat = _fixture.Create<Cat>();
        var replaceResult = new CatUpdateResult
        {
            IsAcknowledged = true,
            ModifiedCount = 1
        };

        _mockContext
            .Setup(c => c.ReplaceOneAsync(cat, It.IsAny<CancellationToken>()))
            .ReturnsAsync(replaceResult);

        // Act
        Func<Task> act = async () => await _catsRepository.UpdateCatAsync(cat, default(CancellationToken));

        // Assert
        await act.Should().NotThrowAsync();
        _mockContext.Verify(c => c.ReplaceOneAsync(cat, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCatAsync_ShouldThrowCatUpdateException_WhenUpdateIsNotAcknowledged()
    {
        // Arrange
        var cat = _fixture.Create<Cat>();
        var replaceResult = new CatUpdateResult
        {
            IsAcknowledged = false,
            ModifiedCount = 0
        };

        _mockContext
            .Setup(c => c.ReplaceOneAsync(cat, It.IsAny<CancellationToken>()))
            .ReturnsAsync(replaceResult);

        // Act & Assert
        Func<Task> act = async () => await _catsRepository.UpdateCatAsync(cat, default(CancellationToken));
        await act.Should().ThrowAsync<CatUpdateException>();
        _mockContext.Verify(c => c.ReplaceOneAsync(cat, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCatAsync_ShouldThrowCatNotFoundException_WhenNoCatIsUpdated()
    {
        // Arrange
        var cat = _fixture.Create<Cat>();
        var replaceResult = new CatUpdateResult
        {
            IsAcknowledged = true,
            ModifiedCount = 0
        };

        _mockContext
            .Setup(c => c.ReplaceOneAsync(cat, It.IsAny<CancellationToken>()))
            .ReturnsAsync(replaceResult);

        // Act & Assert
        Func<Task> act = async () => await _catsRepository.UpdateCatAsync(cat, default(CancellationToken));
        await act.Should().ThrowAsync<CatNotFoundException>();
        _mockContext.Verify(c => c.ReplaceOneAsync(cat, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAvailableCatsAsync_ShouldReturnAvailableCats_WhenThereAreAvailableCats()
    {
        // Arrange
        var availableCats = _fixture.CreateMany<Cat>(3).ToList();

        _mockContext
            .Setup(c => c.GetAvailableCatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(availableCats);

        // Act
        var result = await _catsRepository.GetAvailableCatsAsync(default(CancellationToken));

        // Assert
        result.Should().BeEquivalentTo(availableCats);
        _mockContext.Verify(c => c.GetAvailableCatsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}