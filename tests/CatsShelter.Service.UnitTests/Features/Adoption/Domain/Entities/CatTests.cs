using AutoFixture;
using AutoFixture.AutoMoq;
using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Domain.Exceptions;
using FluentAssertions;

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
        cat.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void RequestAdoption_CatIsUnavailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var cat = _fixture.Build<Cat>()
            .Do(c => c.RequestAdoption())
            .Create();

        // Act & Assert
        Action act = () => cat.RequestAdoption();
        act.Should().Throw<CatUnavailableException>()
            .WithMessage("Cat is not available for adoption.");
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
        cat.IsAvailable.Should().BeTrue();
    }
}