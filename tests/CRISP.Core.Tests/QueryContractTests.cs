using CRISP.Core.Common;
using CRISP.Core.Constants;
//#if (IncludeAuth)
using CRISP.Core.Identity;

//#endif

namespace CRISP.Core.Tests;

/// <summary>
/// Pins the wire shape of the shared query contracts. Uses a local model rather than a feature's
/// so the tests hold whatever features a generated solution keeps.
/// </summary>
public sealed class QueryContractTests
{
    private sealed record Widget : BaseModel
    {
        public string Name { get; init; } = default!;
    }

    private sealed record GetWidgets : PagedQuery<Widget>;

    [Fact]
    public void PagedQuery_WithNullPagingValues_UsesDefaultValues()
    {
        var query = new GetWidgets { Page = null, PageSize = null };

        query.GetPageOrDefault().ShouldBe(Query.Page);
        query.GetPageSizeOrDefault().ShouldBe(Query.PageSize);
        query.ToQueryString().ShouldStartWith("?page=0&pageSize=100", Case.Sensitive);
    }

    [Fact]
    public void PagedQueryToQueryString_WithPagingSortingAndIds_EncodesBaseContract()
    {
        var firstId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var secondId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var query = new GetWidgets
        {
            Page = 2,
            PageSize = 25,
            SortBy = nameof(Widget.Name),
            SortDescending = true,
            Ids = [firstId, secondId],
        };

        string queryString = query.ToQueryString();

        queryString.ShouldContain("page=2", Case.Sensitive);
        queryString.ShouldContain("pageSize=25", Case.Sensitive);
        queryString.ShouldContain("sortBy=Name", Case.Sensitive);
        queryString.ShouldContain("sortDescending=True", Case.Sensitive);
        queryString.ShouldContain($"ids={firstId},{secondId}", Case.Sensitive);
    }

    //#if (IncludeAuth)
    [Fact]
    public void GetUsersToQueryString_WithFilters_EncodesIdentityFilters()
    {
        var query = new GetUsers
        {
            Username = "Ada Lovelace",
            Email = "ada+lab@example.com",
            Role = Roles.Editor,
            IsActive = true,
        };

        string queryString = query.ToQueryString();

        queryString.ShouldContain("username=Ada%20Lovelace", Case.Sensitive);
        queryString.ShouldContain("email=ada%2Blab%40example.com", Case.Sensitive);
        queryString.ShouldContain("role=Editor", Case.Sensitive);
        queryString.ShouldContain("isActive=True", Case.Sensitive);
    }

    //#endif

    [Fact]
    public void PagedResponse_DefaultItems_IsEmptyCollection()
    {
        var response = new PagedResponse<Widget>();

        response.Items.ShouldBeEmpty();
        response.TotalCount.ShouldBe(0);
    }
}
