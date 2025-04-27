using CRISP.Core.Abstractions;
using CRISP.Core.Extensions;
using FluentAssertions;

namespace CRISP.Core.Tests.Extensions;

public class QueryExtensionsTests
{
    [Fact]
    public void ApplyPaging_WithValidParameters_ReturnsCorrectPage()
    {
        // Arrange
        var query = Enumerable.Range(1, 100).AsQueryable();
        int pageNumber = 2;
        int pageSize = 10;

        // Act
        var result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Should().HaveCount(10);
        result.Should().BeEquivalentTo(Enumerable.Range(11, 10));
    }

    [Fact]
    public void ApplyPaging_WithFirstPage_ReturnsFirstItems()
    {
        // Arrange
        var query = Enumerable.Range(1, 50).AsQueryable();
        int pageNumber = 1;
        int pageSize = 10;

        // Act
        var result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Should().HaveCount(10);
        result.Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }

    [Fact]
    public void ApplyPaging_WithLastPage_ReturnsLastItems()
    {
        // Arrange
        var query = Enumerable.Range(1, 25).AsQueryable();
        int pageNumber = 3;
        int pageSize = 10;

        // Act
        var result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Should().HaveCount(5); // Only 5 items on the last page
        result.Should().BeEquivalentTo(Enumerable.Range(21, 5));
    }

    [Fact]
    public void ApplyPaging_WithPageBeyondData_ReturnsEmptyCollection()
    {
        // Arrange
        var query = Enumerable.Range(1, 20).AsQueryable();
        int pageNumber = 10;
        int pageSize = 10;

        // Act
        var result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ApplyPaging_WithNegativePage_UsesFirstPage()
    {
        // Arrange
        var query = Enumerable.Range(1, 50).AsQueryable();
        int pageNumber = -1;
        int pageSize = 10;

        // Act
        var result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Should().HaveCount(10);
        result.Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }

    [Fact]
    public void ApplyPaging_WithZeroPage_UsesFirstPage()
    {
        // Arrange
        var query = Enumerable.Range(1, 50).AsQueryable();
        int pageNumber = 0;
        int pageSize = 10;

        // Act
        var result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Should().HaveCount(10);
        result.Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }

    [Fact]
    public void ApplyPaging_WithNegativePageSize_UsesDefaultPageSize()
    {
        // Arrange
        var query = Enumerable.Range(1, 100).AsQueryable();
        int pageNumber = 1;
        int pageSize = -10;

        // Act
        var result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Should().HaveCount(10); // Default page size is 10
        result.Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }

    [Fact]
    public void ApplyPaging_WithZeroPageSize_UsesDefaultPageSize()
    {
        // Arrange
        var query = Enumerable.Range(1, 100).AsQueryable();
        int pageNumber = 1;
        int pageSize = 0;

        // Act
        var result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Should().HaveCount(10); // Default page size is 10
        result.Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }

    [Fact]
    public void ToPaginatedResult_ReturnsCorrectResult()
    {
        // Arrange
        var query = Enumerable.Range(1, 100).AsQueryable();
        int pageNumber = 2;
        int pageSize = 10;

        // Act
        var result = query.ToPaginatedResult(pageNumber, pageSize);

        // Assert
        result.Items.Should().HaveCount(10);
        result.Items.Should().BeEquivalentTo(Enumerable.Range(11, 10));
        result.TotalCount.Should().Be(100);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(10);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void ApplyFilter_WithCustomFilterBase_AppliesPagination()
    {
        // Arrange
        var query = Enumerable.Range(1, 100).AsQueryable();
        var filter = new TestFilter
        {
            PageNumber = 2,
            PageSize = 10
        };

        // Act
        var result = query.ApplyFilter(filter).ToList();

        // Assert
        result.Should().HaveCount(10);
        result.Should().BeEquivalentTo(Enumerable.Range(11, 10));
    }

    [Fact]
    public void ToPaginatedResult_WithCustomFilterBase_ReturnsPaginatedResult()
    {
        // Arrange
        var query = Enumerable.Range(1, 100).AsQueryable();
        var filter = new TestFilter
        {
            PageNumber = 2,
            PageSize = 10
        };

        // Act
        var result = query.ToPaginatedResult(filter);

        // Assert
        result.Items.Should().HaveCount(10);
        result.Items.Should().BeEquivalentTo(Enumerable.Range(11, 10));
        result.TotalCount.Should().Be(100);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    // Custom filter class for testing
    private class TestFilter : FilterBase
    {
        public string? TestProperty { get; set; }
    }
}