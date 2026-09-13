using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CRISP.Core.Common;
//#if (IncludeAuth)
using CRISP.Core.Identity;
//#endif
using CRISP.Core.Items;
using CRISP.Server.Tests.TestSupport;

namespace CRISP.Server.Tests.Integration;

/// <summary>
/// The Item slice end to end: real pipeline, real MySQL, real interceptors. This is the worked
/// example a new feature copies, so it is also the suite a new feature's tests copy.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ItemEndpointTests(IntegrationTestFixture fixture)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<HttpClient> EditorAsync()
    {
        await fixture.ResetDatabaseAsync();
        string? editor = null;
//#if (IncludeAuth)
        editor = await fixture.SeedUserAsync(Roles.Editor);
//#endif
        return fixture.CreateClient(editor);
    }

    private static async Task<Guid> CreateAsync(
        HttpClient client,
        string name,
        string? owner = null
    )
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/Items",
            new CreateItem { Name = name, Owner = owner },
            Ct
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<Guid>(Json, Ct);
    }

//#if (IncludeAuth)
    [Fact(Timeout = 120_000)]
    public async Task Anonymous_IsRefused()
    {
        await fixture.ResetDatabaseAsync();
        var client = fixture.CreateClient(asUser: null);

        var response = await client.GetAsync("/api/v1/Items", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
//#endif

    [Fact(Timeout = 120_000)]
    public async Task Create_ReturnsTheIdAndALocation_AndTheItemCanBeRead()
    {
        var client = await EditorAsync();

        var response = await client.PostAsJsonAsync(
            "/api/v1/Items",
            new CreateItem
            {
                Name = "Wrench",
                Description = "10mm",
                Owner = "Shop",
            },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>(Json, Ct);
        response.Headers.Location.ShouldNotBeNull().ToString().ShouldEndWith($"/Items/{id}");

        var item = await client.GetFromJsonAsync<Item>($"/api/v1/Items/{id}", Json, Ct);
        item.ShouldNotBeNull();
        item.Name.ShouldBe("Wrench");
        item.Description.ShouldBe("10mm");
        item.Owner.ShouldBe("Shop");
        item.IsActive.ShouldBeTrue();
    }

    // The audit interceptor stamps CreatedBy from the current user; nothing in the handler sets it.
    [Fact(Timeout = 120_000)]
    public async Task Create_StampsTheAuditFieldsFromTheCaller()
    {
        var client = await EditorAsync();
        var id = await CreateAsync(client, "Wrench");

        var item = await client.GetFromJsonAsync<Item>($"/api/v1/Items/{id}", Json, Ct);

        item.ShouldNotBeNull();
        var actor = "System";
//#if (IncludeAuth)
        actor = "editor@crisp.test";
//#endif
        item.CreatedBy.ShouldBe(actor);
        item.CreatedOn.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-5));
    }

    // Shape validation is the group-level filter's job. A blank name never reaches the handler and
    // comes back as a 400 ValidationProblem naming the property, not as a 500 from the database.
    [Fact(Timeout = 120_000)]
    public async Task Create_WithABlankName_IsAValidationProblem()
    {
        var client = await EditorAsync();

        var response = await client.PostAsJsonAsync(
            "/api/v1/Items",
            new CreateItem { Name = "" },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        problem.GetProperty("errors").TryGetProperty("Name", out _).ShouldBeTrue();
    }

    // A uniqueness rule needs the database, so it is enforced in the service and reported as a
    // RejectedException. Its message is the point: the caller is told which name is taken.
    [Fact(Timeout = 120_000)]
    public async Task Create_WithADuplicateName_IsAConflictThatNamesTheDuplicate()
    {
        var client = await EditorAsync();
        await CreateAsync(client, "Wrench");

        var response = await client.PostAsJsonAsync(
            "/api/v1/Items",
            new CreateItem { Name = "Wrench" },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        problem
            .GetProperty("detail")
            .GetString()
            .ShouldBe("Item with name 'Wrench' already exists.");
    }

    [Fact(Timeout = 120_000)]
    public async Task Get_AnUnknownId_IsNotFoundAndSaysWhich()
    {
        var client = await EditorAsync();
        var id = Guid.NewGuid();

        var response = await client.GetAsync($"/api/v1/Items/{id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        problem.GetProperty("detail").GetString().ShouldBe($"Item with ID '{id}' not found.");
    }

    // The bug RouteIdBinder exists to stop: the body carries no id, the URL does. Before the binder
    // this was a 400 for a missing Id the caller had already supplied.
    [Fact(Timeout = 120_000)]
    public async Task Update_TakesTheIdFromTheRoute_NotTheBody()
    {
        var client = await EditorAsync();
        var id = await CreateAsync(client, "Wrench");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/Items/{id}",
            new
            {
                name = "Torque wrench",
                description = "1/2 inch",
                isActive = true,
            },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var item = await client.GetFromJsonAsync<Item>($"/api/v1/Items/{id}", Json, Ct);
        item.ShouldNotBeNull().Name.ShouldBe("Torque wrench");
    }

    // The route is the authority. A body that names a different item is not trusted over the URL.
    [Fact(Timeout = 120_000)]
    public async Task Update_IgnoresABodyIdThatDisagreesWithTheRoute()
    {
        var client = await EditorAsync();
        var target = await CreateAsync(client, "Wrench");
        var other = await CreateAsync(client, "Hammer");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/Items/{target}",
            new UpdateItem
            {
                Id = other,
                Name = "Renamed",
                IsActive = true,
            },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await client.GetFromJsonAsync<Item>($"/api/v1/Items/{target}", Json, Ct))!.Name.ShouldBe(
            "Renamed"
        );
        (await client.GetFromJsonAsync<Item>($"/api/v1/Items/{other}", Json, Ct))!.Name.ShouldBe(
            "Hammer"
        );
    }

    [Fact(Timeout = 120_000)]
    public async Task Update_ToAnotherItemsName_IsAConflict()
    {
        var client = await EditorAsync();
        var id = await CreateAsync(client, "Wrench");
        await CreateAsync(client, "Hammer");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/Items/{id}",
            new UpdateItem { Name = "Hammer", IsActive = true },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    // The activity feed is written by the audit interceptor on SaveChanges, not by the handler, and
    // the rename message is the one place the Describe() formatting of scalars is visible end to end.
    [Fact(Timeout = 120_000)]
    public async Task Activity_RecordsTheCreateAndTheRename()
    {
        var client = await EditorAsync();
        var id = await CreateAsync(client, "Wrench");
        await client.PutAsJsonAsync(
            $"/api/v1/Items/{id}",
            new UpdateItem { Name = "Torque wrench", IsActive = true },
            Ct
        );

        var messages = await client.GetFromJsonAsync<List<JsonElement>>(
            $"/api/v1/Items/{id}/activity",
            Json,
            Ct
        );

        messages.ShouldNotBeNull();
        var contents = messages
            .Select(m =>
                string.Join(
                    "\n",
                    m.GetProperty("content").EnumerateArray().Select(l => l.GetString())
                )
            )
            .ToList();
        var actor = "System";
//#if (IncludeAuth)
        actor = "editor@crisp.test";
//#endif
        contents.ShouldContain(c => c.Contains($"{actor} Added Item"));
        contents.ShouldContain(c => c.Contains("Name: 'Wrench' → 'Torque wrench'"));
    }

    // Delete is a soft delete: the row stays, the query filter hides it, the API says 404.
    [Fact(Timeout = 120_000)]
    public async Task Delete_HidesTheItemFromEveryRead()
    {
        var client = await EditorAsync();
        var id = await CreateAsync(client, "Wrench");
        await CreateAsync(client, "Hammer");

        var response = await client.DeleteAsync($"/api/v1/Items/{id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/v1/Items/{id}", Ct)).StatusCode.ShouldBe(
            HttpStatusCode.NotFound
        );
        var page = await client.GetFromJsonAsync<PagedResponse<Item>>("/api/v1/Items", Json, Ct);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(1);
        page.Items.Single().Name.ShouldBe("Hammer");
    }

    [Fact(Timeout = 120_000)]
    public async Task List_FiltersAndPages()
    {
        var client = await EditorAsync();
        foreach (var name in new[] { "Wrench A", "Wrench B", "Wrench C", "Hammer" })
            await CreateAsync(client, name, owner: name.StartsWith("Wrench") ? "Shop" : "Site");

        var page = await client.GetFromJsonAsync<PagedResponse<Item>>(
            "/api/v1/Items?name=Wrench&page=0&pageSize=2&sortBy=Name",
            Json,
            Ct
        );

        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(3);
        page.Items.Select(i => i.Name).ShouldBe(["Wrench A", "Wrench B"]);
    }

    [Fact(Timeout = 120_000)]
    public async Task OwnerNames_AreDistinctSuggestions()
    {
        var client = await EditorAsync();
        await CreateAsync(client, "A", owner: "Shop");
        await CreateAsync(client, "B", owner: "Shop");
        await CreateAsync(client, "C", owner: "Site");

        var names = await client.GetFromJsonAsync<List<AutoComplete>>(
            "/api/v1/Items/owners",
            Json,
            Ct
        );

        names.ShouldNotBeNull().Select(n => n.Name).Order().ShouldBe(["Shop", "Site"]);
    }
}
