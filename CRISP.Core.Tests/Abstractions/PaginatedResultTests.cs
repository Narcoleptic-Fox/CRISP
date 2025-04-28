using CRISP.Core.Abstractions;

namespace CRISP.Core.Tests.Abstractions;

public class PaginatedResultTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Act
        PaginatedResult<string> result = new()
        {
            Items = ["Item1", "Item2"],
            TotalCount = 10,
            PageNumber = 2,
            PageSize = 2
        };

        // Assert
        result.Items.Count().ShouldBe(2);
        result.Items.ShouldContain("Item1");
        result.Items.ShouldContain("Item2");
        result.TotalCount.ShouldBe(10);
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(2);
    }

    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        // Arrange
        PaginatedResult<string> result = new()
        {
            TotalCount = 10,
            PageSize = 3
        };

        // Act & Assert
        result.TotalPages.ShouldBe(4); // Ceiling of 10/3 = 4
    }

    [Fact]
    public void TotalPages_WithExactDivision_CalculatesCorrectly()
    {
        // Arrange
        PaginatedResult<string> result = new()
        {
            TotalCount = 10,
            PageSize = 5
        };

        // Act & Assert
        result.TotalPages.ShouldBe(2); // 10/5 = 2
    }

    [Fact]
    public void HasPreviousPage_WhenOnFirstPage_ReturnsFalse()
    {
        // Arrange
        PaginatedResult<string> result = new()
        {
            PageNumber = 1
        };

        // Act & Assert
        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenNotOnFirstPage_ReturnsTrue()
    {
        // Arrange
        PaginatedResult<string> result = new()
        {
            PageNumber = 2
        };

        // Act & Assert
        result.HasPreviousPage.ShouldBeTrue();
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_ReturnsFalse()
    {
        // Arrange
        PaginatedResult<string> result = new()
        {
            PageNumber = 5,
            PageSize = 2,
            TotalCount = 10
        };

        // Act & Assert
        result.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public void HasNextPage_WhenNotOnLastPage_ReturnsTrue()
    {
        // Arrange
        PaginatedResult<string> result = new()
        {
            PageNumber = 2,
            PageSize = 2,
            TotalCount = 10
        };

        // Act & Assert
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public void Create_ReturnsNewInstanceWithCorrectProperties()
    {
        // Arrange
        List<string> items = ["Item1", "Item2"];
        const int totalCount = 10;
        const int pageNumber = 2;
        const int pageSize = 2;

        // Act
        PaginatedResult<string> result = PaginatedResult<string>.Create(items, totalCount, pageNumber, pageSize);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEquivalentTo(items);
        result.TotalCount.ShouldBe(totalCount);
        result.PageNumber.ShouldBe(pageNumber);
        result.PageSize.ShouldBe(pageSize);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultProperties()
    {
        // Act
        PaginatedResult<string> result = new();

        // Assert
        result.Items.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.PageNumber.ShouldBe(0);
        result.PageSize.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
        result.HasPreviousPage.ShouldBeFalse();
        result.HasNextPage.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0, 5, 0)]  // Edge case: zero total count
    [InlineData(10, 0, int.MaxValue)] // Edge case: zero page size - division by zero results in Int32.MaxValue
    public void TotalPages_WithEdgeCases_HandlesCorrectly(int totalCount, int pageSize, int expectedPages)
    {
        // Arrange
        PaginatedResult<string> result = new()
        {
            TotalCount = totalCount,
            PageSize = pageSize
        };

        // Act & Assert
        result.TotalPages.ShouldBe(expectedPages);
    }
}
