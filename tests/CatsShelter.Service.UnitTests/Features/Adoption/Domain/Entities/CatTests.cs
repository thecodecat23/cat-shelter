using AutoFixture;
using AutoFixture.AutoMoq;
using CatsShelter.Service.Features.Adoption.Domain.Entities;

namespace CatsShelter.Service.UnitTests.Features.Adoption.Domain.Entities;

public class CatTests
{
    private readonly IFixture _fixture;

    public CatTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void RequestAdoption_CatIsAvailable_ShouldMakeCatUnavailable()
    {
        // Arrange
        var cat = _fixture.Create<Cat>();

        // Act
        cat.RequestAdoption();

        // Assert
        Assert.False(cat.IsAvailable);
    }

    [Fact]
    public void RequestAdoption_CatIsUnavailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var cat = _fixture.Build<Cat>()
            .Do(c => c.RequestAdoption())
            .Create();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => cat.RequestAdoption());
        Assert.Equal("Cat is not available for adoption.", exception.Message);
    }

    [Fact]
    public void CancelAdoption_CatIsUnavailable_ShouldMakeCatAvailableAgain()
    {
        // Arrange
        var cat = _fixture.Build<Cat>()
            .Do(c => c.RequestAdoption())
            .Create();

        // Act
        cat.CancelAdoption();

        // Assert
        Assert.True(cat.IsAvailable);
    }
}
