using CRISP.Core.Abstractions;
using FluentAssertions;

namespace CRISP.Core.Tests.Abstractions;

public class PaginatedResultTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Act
        var result = new PaginatedResult<string>
        {
            Items = new List<string> { "Item1", "Item2" },
            TotalCount = 10,
            PageNumber = 2,
            PageSize = 2
        };

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain("Item1");
        result.Items.Should().Contain("Item2");
        result.TotalCount.Should().Be(10);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        // Arrange
        var result = new PaginatedResult<string>
        {
            TotalCount = 10,
            PageSize = 3
        };

        // Act & Assert
        result.TotalPages.Should().Be(4); // Ceiling of 10/3 = 4
    }

    [Fact]
    public void TotalPages_WithExactDivision_CalculatesCorrectly()
    {
        // Arrange
        var result = new PaginatedResult<string>
        {
            TotalCount = 10,
            PageSize = 5
        };

        // Act & Assert
        result.TotalPages.Should().Be(2); // 10/5 = 2
    }

    [Fact]
    public void HasPreviousPage_WhenOnFirstPage_ReturnsFalse()
    {
        // Arrange
        var result = new PaginatedResult<string>
        {
            PageNumber = 1
        };

        // Act & Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenNotOnFirstPage_ReturnsTrue()
    {
        // Arrange
        var result = new PaginatedResult<string>
        {
            PageNumber = 2
        };

        // Act & Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_ReturnsFalse()
    {
        // Arrange
        var result = new PaginatedResult<string>
        {
            PageNumber = 5,
            PageSize = 2,
            TotalCount = 10
        };

        // Act & Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_WhenNotOnLastPage_ReturnsTrue()
    {
        // Arrange
        var result = new PaginatedResult<string>
        {
            PageNumber = 2,
            PageSize = 2,
            TotalCount = 10
        };

        // Act & Assert
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Create_ReturnsNewInstanceWithCorrectProperties()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2" };
        const int totalCount = 10;
        const int pageNumber = 2;
        const int pageSize = 2;

        // Act
        var result = PaginatedResult<string>.Create(items, totalCount, pageNumber, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
    }
    
    [Fact]
    public void DefaultConstructor_SetsDefaultProperties()
    {
        // Act
        var result = new PaginatedResult<string>();
        
        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(0);
        result.PageSize.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(0, 5, 0)]  // Edge case: zero total count
    [InlineData(10, 0, int.MaxValue)] // Edge case: zero page size - division by zero results in Int32.MaxValue
    public void TotalPages_WithEdgeCases_HandlesCorrectly(int totalCount, int pageSize, int expectedPages)
    {
        // Arrange
        var result = new PaginatedResult<string>
        {
            TotalCount = totalCount,
            PageSize = pageSize
        };
        
        // Act & Assert
        result.TotalPages.Should().Be(expectedPages);
    }
}
