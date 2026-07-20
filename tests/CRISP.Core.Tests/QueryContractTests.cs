using ACM.Core.Assets;
using ACM.Core.Common;
using ACM.Core.Constants;
using ACM.Core.Identity;

namespace ACM.Core.Tests;

public sealed class QueryContractTests
{
    [Fact]
    public void PagedQuery_WithNullPagingValues_UsesDefaultValues()
    {
        var query = new GetCalibrationAssets
        {
            Page = null,
            PageSize = null,
        };

        query.GetPageOrDefault().ShouldBe(Query.Page);
        query.GetPageSizeOrDefault().ShouldBe(Query.PageSize);
        query.ToQueryString().ShouldStartWith("?page=0&pageSize=100", Case.Sensitive);
    }

    [Fact]
    public void GetAssetsToQueryString_WithFilters_EncodesSdkQueryContract()
    {
        var firstId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var secondId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var calibrationStart = new DateTimeOffset(2026, 5, 8, 12, 30, 0, TimeSpan.Zero);
        var query = new GetCalibrationAssets
        {
            Page = 2,
            PageSize = 25,
            SortBy = nameof(Asset.Name),
            SortDescending = true,
            Ids = [firstId, secondId],
            Name = "Gauge A/B",
            Instrument = "Pressure & Flow",
            CalibrationStartDate = calibrationStart,
            IsRetired = false,
            CalibrationStatus = CalibrationStatus.Overdue,
        };

        string queryString = query.ToQueryString();

        queryString.ShouldContain("page=2", Case.Sensitive);
        queryString.ShouldContain("pageSize=25", Case.Sensitive);
        queryString.ShouldContain("sortBy=Name", Case.Sensitive);
        queryString.ShouldContain("sortDescending=True", Case.Sensitive);
        queryString.ShouldContain($"ids={firstId},{secondId}", Case.Sensitive);
        queryString.ShouldContain("Name=Gauge%20A%2FB", Case.Sensitive);
        queryString.ShouldContain("Instrument=Pressure%20%26%20Flow", Case.Sensitive);
        queryString.ShouldContain($"CalibrationStartDate={Uri.EscapeDataString(calibrationStart.ToString("o"))}", Case.Sensitive);
        queryString.ShouldContain("IsRetired=false", Case.Sensitive);
        queryString.ShouldContain("CalibrationStatus=Overdue", Case.Sensitive);
    }

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

    [Fact]
    public void PagedResponse_DefaultItems_IsEmptyCollection()
    {
        var response = new PagedResponse<Asset>();

        response.Items.ShouldBeEmpty();
        response.TotalCount.ShouldBe(0);
    }
}