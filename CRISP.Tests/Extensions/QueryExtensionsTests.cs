namespace CRISP.Tests.Extensions;

public class QueryExtensionsTests
{
    [Fact]
    public void ApplyPaging_WithValidParameters_ReturnsCorrectPage()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 100).AsQueryable();
        int pageNumber = 2;
        int pageSize = 10;

        // Act
        List<int> result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Count().ShouldBe(10);
        result.ShouldBeEquivalentTo(Enumerable.Range(11, 10).ToList());
    }

    [Fact]
    public void ApplyPaging_WithFirstPage_ReturnsFirstItems()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 50).AsQueryable();
        int pageNumber = 1;
        int pageSize = 10;

        // Act
        List<int> result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Count().ShouldBe(10);
        result.ShouldBeEquivalentTo(Enumerable.Range(1, 10).ToList());
    }

    [Fact]
    public void ApplyPaging_WithLastPage_ReturnsLastItems()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 25).AsQueryable();
        int pageNumber = 3;
        int pageSize = 10;

        // Act
        List<int> result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Count().ShouldBe(5); // Only 5 items on the last page
        result.ShouldBeEquivalentTo(Enumerable.Range(21, 5).ToList());
    }

    [Fact]
    public void ApplyPaging_WithPageBeyondData_ReturnsEmptyCollection()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 20).AsQueryable();
        int pageNumber = 10;
        int pageSize = 10;

        // Act
        List<int> result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ApplyPaging_WithNegativePage_UsesFirstPage()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 50).AsQueryable();
        int pageNumber = -1;
        int pageSize = 10;

        // Act
        List<int> result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Count().ShouldBe(10);
        result.ShouldBeEquivalentTo(Enumerable.Range(1, 10).ToList());
    }

    [Fact]
    public void ApplyPaging_WithZeroPage_UsesFirstPage()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 50).AsQueryable();
        int pageNumber = 0;
        int pageSize = 10;

        // Act
        List<int> result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Count().ShouldBe(10);
        result.ShouldBeEquivalentTo(Enumerable.Range(1, 10).ToList());
    }

    [Fact]
    public void ApplyPaging_WithNegativePageSize_UsesDefaultPageSize()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 100).AsQueryable();
        int pageNumber = 1;
        int pageSize = -10;

        // Act
        List<int> result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Count().ShouldBe(10); // Default page size is 10
        result.ShouldBeEquivalentTo(Enumerable.Range(1, 10).ToList());
    }

    [Fact]
    public void ApplyPaging_WithZeroPageSize_UsesDefaultPageSize()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 100).AsQueryable();
        int pageNumber = 1;
        int pageSize = 0;

        // Act
        List<int> result = query.ApplyPaging(pageNumber, pageSize).ToList();

        // Assert
        result.Count().ShouldBe(10); // Default page size is 10
        result.ShouldBeEquivalentTo(Enumerable.Range(1, 10).ToList());
    }

    [Fact]
    public void ToPaginatedResult_ReturnsCorrectResult()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 100).AsQueryable();
        int pageNumber = 2;
        int pageSize = 10;

        // Act
        PagedResult<int> result = query.ToPaginatedResult(pageNumber, pageSize);

        // Assert
        result.Items.Count().ShouldBe(10);
        result.Items.ToList().ShouldBeEquivalentTo(Enumerable.Range(11, 10).ToList());
        result.TotalCount.ShouldBe(100);
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(10);
        result.TotalPages.ShouldBe(10);
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public void ToPaginatedResult_WithCustomFilterBase_ReturnsPaginatedResult()
    {
        // Arrange
        IQueryable<int> query = Enumerable.Range(1, 100).AsQueryable();
        TestFilter filter = new()
        {
            Page = 2,
            PageSize = 10
        };

        // Act
        PagedResult<int> result = query.ToPaginatedResult(filter);

        // Assert
        result.Items.Count().ShouldBe(10);
        result.Items.ToList().ShouldBeEquivalentTo(Enumerable.Range(11, 10).ToList());
        result.TotalCount.ShouldBe(100);
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(10);
    }

    // Custom filter class for testing
    private class TestFilter : FilterBase
    {
        public string? TestProperty { get; set; }
    }
}