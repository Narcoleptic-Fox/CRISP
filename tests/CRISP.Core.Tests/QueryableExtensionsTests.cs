using ACM.Core.Common;

namespace ACM.Core.Tests;

public sealed class QueryableExtensionsTests
{
    [Fact]
    public void SortBy_WithAscendingPropertyName_OrdersByRequestedProperty()
    {
        var items = new[]
        {
            new QueryableTestModel { Id = Guid.NewGuid(), Name = "Gamma" },
            new QueryableTestModel { Id = Guid.NewGuid(), Name = "Alpha" },
            new QueryableTestModel { Id = Guid.NewGuid(), Name = "Beta" },
        }.AsQueryable();

        string[] orderedNames = items.SortBy(nameof(QueryableTestModel.Name)).Select(item => item.Name).ToArray();

        orderedNames.ShouldBe(["Alpha", "Beta", "Gamma"]);
    }

    [Fact]
    public void SortBy_WithDescendingPropertyName_OrdersDescendingByRequestedProperty()
    {
        var items = new[]
        {
            new QueryableTestModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "One" },
            new QueryableTestModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "Three" },
            new QueryableTestModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Two" },
        }.AsQueryable();

        Guid[] orderedIds = items.SortBy(nameof(QueryableTestModel.Id), descending: true).Select(item => item.Id).ToArray();

        orderedIds.ShouldBe(
            [
                Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
            ]);
    }

    [Fact]
    public void SortBy_WithUnknownProperty_FallsBackToId()
    {
        var items = new[]
        {
            new QueryableTestModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Two" },
            new QueryableTestModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "One" },
        }.AsQueryable();

        Guid[] orderedIds = items.SortBy("MissingProperty").Select(item => item.Id).ToArray();

        orderedIds.ShouldBe(
            [
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
            ]);
    }

    [Fact]
    public void ToPaged_WithExplicitPageAndSize_ReturnsRequestedWindow()
    {
        var items = Enumerable.Range(1, 10)
            .Select(index => new QueryableTestModel { Id = Guid.NewGuid(), Name = $"Item {index}", Number = index })
            .AsQueryable();

        int[] page = items.ToPaged(page: 1, pageSize: 3).Select(item => item.Number).ToArray();

        page.ShouldBe([4, 5, 6]);
    }

    private sealed record QueryableTestModel : BaseModel
    {
        public string Name { get; init; } = string.Empty;

        public int Number { get; init; }
    }
}