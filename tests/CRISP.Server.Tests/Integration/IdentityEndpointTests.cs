using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CRISP.Core.Common;
using CRISP.Core.Identity;
using CRISP.Server.Tests.TestSupport;

namespace CRISP.Server.Tests.Integration;

/// <summary>
/// The role policies end to end. The test scheme fabricates only an identity; the role claim comes
/// from the real enricher reading the real Users table, so these cover stored role → claim → policy
/// → endpoint, which is the chain a fake principal would have skipped.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
[Trait("Category", "Integration")]
public sealed class IdentityEndpointTests(IntegrationTestFixture fixture)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact(Timeout = 120_000)]
    public async Task Me_ReturnsTheSeededUserWithItsRole()
    {
        await fixture.ResetDatabaseAsync();
        var client = fixture.CreateClient(await fixture.SeedUserAsync(Roles.Viewer));

        var me = await client.GetFromJsonAsync<User>("/api/v1/Users/me", Json, Ct);

        me.ShouldNotBeNull();
        me.Email.ShouldBe("viewer@crisp.test");
        me.Role.ShouldBe(Roles.Viewer);
    }

    // An identity the provider vouches for but nobody has provisioned is authenticated and holds
    // no role. /me is the one endpoint that must still answer, so the client can say so.
    [Fact(Timeout = 120_000)]
    public async Task Me_ForAnUnprovisionedIdentity_SaysSoRatherThanFailingAuth()
    {
        await fixture.ResetDatabaseAsync();
        var client = fixture.CreateClient("stranger@crisp.test");

        var response = await client.GetAsync("/api/v1/Users/me", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        problem.GetProperty("code").GetString().ShouldBe("unprovisioned");
    }

    [Fact(Timeout = 120_000)]
    public async Task AnUnprovisionedIdentity_HoldsNoRole_AndIsRefusedByEveryPolicy()
    {
        await fixture.ResetDatabaseAsync();
        var client = fixture.CreateClient("stranger@crisp.test");

        (await client.GetAsync("/api/v1/Users/names", Ct)).StatusCode.ShouldBe(
            HttpStatusCode.Forbidden
        );
        (await client.GetAsync("/api/v1/Users", Ct)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    // An inactive account is not "no account": the row exists, but the enricher refuses to grant
    // it a role, so it is treated exactly like an unprovisioned identity.
    [Fact(Timeout = 120_000)]
    public async Task AnInactiveUser_IsRefusedLikeAnUnprovisionedOne()
    {
        await fixture.ResetDatabaseAsync();
        var admin = fixture.CreateClient(await fixture.SeedUserAsync(Roles.Admin));
        var created = await admin.PostAsJsonAsync(
            "/api/v1/Users",
            new CreateUser
            {
                Username = "dormant",
                Email = "dormant@crisp.test",
                Role = Roles.Admin,
                IsActive = false,
            },
            Ct
        );
        created.StatusCode.ShouldBe(HttpStatusCode.Created);

        var dormant = fixture.CreateClient("dormant@crisp.test");

        (await dormant.GetAsync("/api/v1/Users", Ct)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(Timeout = 120_000)]
    public async Task ReadAccess_AdmitsAViewer_ButAdminAccessDoesNot()
    {
        await fixture.ResetDatabaseAsync();
        var viewer = fixture.CreateClient(await fixture.SeedUserAsync(Roles.Viewer));

        (await viewer.GetAsync("/api/v1/Users/names", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await viewer.GetAsync("/api/v1/Users", Ct)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var create = await viewer.PostAsJsonAsync(
            "/api/v1/Users",
            new CreateUser
            {
                Username = "x",
                Email = "x@crisp.test",
                Role = Roles.Viewer,
                IsActive = true,
            },
            Ct
        );
        create.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(Timeout = 120_000)]
    public async Task Admin_CanProvisionAUser_WhoIsThenRecognised()
    {
        await fixture.ResetDatabaseAsync();
        var admin = fixture.CreateClient(await fixture.SeedUserAsync(Roles.Admin));

        var response = await admin.PostAsJsonAsync(
            "/api/v1/Users",
            new CreateUser
            {
                Username = "newbie",
                Email = "newbie@crisp.test",
                Role = Roles.Editor,
                IsActive = true,
            },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var newbie = fixture.CreateClient("newbie@crisp.test");
        var me = await newbie.GetFromJsonAsync<User>("/api/v1/Users/me", Json, Ct);
        me.ShouldNotBeNull().Role.ShouldBe(Roles.Editor);
    }

    [Fact(Timeout = 120_000)]
    public async Task Admin_CannotProvisionADuplicateEmail()
    {
        await fixture.ResetDatabaseAsync();
        var admin = fixture.CreateClient(await fixture.SeedUserAsync(Roles.Admin));

        var response = await admin.PostAsJsonAsync(
            "/api/v1/Users",
            new CreateUser
            {
                Username = "other",
                Email = "admin@crisp.test",
                Role = Roles.Viewer,
                IsActive = true,
            },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        problem
            .GetProperty("detail")
            .GetString()
            .ShouldBe("User with email 'admin@crisp.test' already exists.");
    }

    [Fact(Timeout = 120_000)]
    public async Task Admin_CanListUsersPaged()
    {
        await fixture.ResetDatabaseAsync();
        var admin = fixture.CreateClient(await fixture.SeedUserAsync(Roles.Admin));
        await fixture.SeedUserAsync(Roles.Viewer);
        await fixture.SeedUserAsync(Roles.Editor);

        var page = await admin.GetFromJsonAsync<PagedResponse<User>>(
            "/api/v1/Users?pageSize=2&sortBy=Email",
            Json,
            Ct
        );

        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(3);
        page.Items.Select(u => u.Email).ShouldBe(["admin@crisp.test", "editor@crisp.test"]);
    }
}
