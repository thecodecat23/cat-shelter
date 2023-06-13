using AutoFixture;
using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Repositories;
using CatsShelter.Service.Features.Adoption.Repositories.Exceptions;
using MongoDB.Driver;
using Moq;

namespace CatsShelter.Service.UnitTests.Features.Adoption.Repositories;

public class CatRepositoryTests
{
    private readonly Mock<IMongoCollection<Cat>> _mockCollection;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly ICatsRepository _catsRepository;
    private readonly Fixture _fixture;

    public CatRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Cat>>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockClient = new Mock<IMongoClient>();
        _fixture = new Fixture();

        _mockClient
            .Setup(c => c.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
            .Returns(_mockDatabase.Object);

        _mockDatabase
            .Setup(db => db.GetCollection<Cat>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockCollection.Object);

        _catsRepository = new CatsRepository(_mockClient.Object, "DatabaseName", "CollectionName");
    }

    [Fact]
    public async Task GetCatByIdAsync_ShouldReturnCat_WhenExists()
    {
        // Arrange
        var id = _fixture.Create<string>();
        var cat = _fixture.Create<Cat>();
        var filter = Builders<Cat>.Filter.Eq(c => c.Id, id);

        _mockCollection
            .Setup(c => c.FindSync(filter, null, default(CancellationToken)))
            .Returns(new Mock<IAsyncCursor<Cat>>(MockBehavior.Strict).Object);

        // Act
        var result = await _catsRepository.GetCatByIdAsync(id);

        // Assert
        Assert.Equal(cat, result);
        _mockCollection.Verify(c => c.FindSync(filter, null, default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task GetCatByIdAsync_ShouldThrowCatNotFoundException_WhenCatDoesNotExist()
    {
        // Arrange
        var id = _fixture.Create<string>();
        var filter = Builders<Cat>.Filter.Eq(c => c.Id, id);

        _mockCollection
            .Setup(c => c.FindSync(filter, null, default(CancellationToken)))
            .Returns((IAsyncCursor<Cat>)null);

        // Act & Assert
        await Assert.ThrowsAsync<CatNotFoundException>(() => _catsRepository.GetCatByIdAsync(id));
        _mockCollection.Verify(c => c.FindSync(filter, null, default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task UpdateCatAsync_ShouldReturnTrue_WhenUpdateIsSuccessful()
    {
        // Arrange
        var cat = _fixture.Create<Cat>();
        var filter = Builders<Cat>.Filter.Eq(c => c.Id, cat.Id);

        // Mock ReplaceOneResult
        var mockReplaceOneResult = new Mock<ReplaceOneResult>();
        mockReplaceOneResult.SetupGet(r => r.IsAcknowledged).Returns(true);
        mockReplaceOneResult.SetupGet(r => r.ModifiedCount).Returns(1L);

        _mockCollection
            .Setup(c => c.ReplaceOneAsync(filter, cat, It.IsAny<ReplaceOptions>(), default(CancellationToken)))
            .Returns(Task.FromResult(mockReplaceOneResult.Object));

        // Act
        var result = await _catsRepository.UpdateCatAsync(cat);

        // Assert
        Assert.True(result);
        _mockCollection.Verify(c => c.ReplaceOneAsync(filter, cat, It.IsAny<ReplaceOptions>(), default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task UpdateCatAsync_ShouldThrowCatNotFoundException_WhenCatDoesNotExist()
    {
        // Arrange
        var cat = _fixture.Create<Cat>();
        var filter = Builders<Cat>.Filter.Eq(c => c.Id, cat.Id);

        // Mock ReplaceOneResult
        var mockReplaceOneResult = new Mock<ReplaceOneResult>();
        mockReplaceOneResult.SetupGet(r => r.IsAcknowledged).Returns(false);
        mockReplaceOneResult.SetupGet(r => r.ModifiedCount).Returns(0L);

        _mockCollection
            .Setup(c => c.ReplaceOneAsync(filter, cat, It.IsAny<ReplaceOptions>(), default(CancellationToken)))
            .Returns(Task.FromResult(mockReplaceOneResult.Object));

        // Act & Assert
        await Assert.ThrowsAsync<CatNotFoundException>(() => _catsRepository.UpdateCatAsync(cat));
        _mockCollection.Verify(c => c.ReplaceOneAsync(filter, cat, It.IsAny<ReplaceOptions>(), default(CancellationToken)), Times.Once);
    }
}