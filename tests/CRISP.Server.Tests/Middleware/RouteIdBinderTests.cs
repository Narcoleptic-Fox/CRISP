using CRISP.Server.Middleware;

namespace CRISP.Server.Tests.Middleware;

[Trait("Category", "Unit")]
public class RouteIdBinderTests
{
    private sealed record Command
    {
        public Guid Id { get; init; }
        public Guid ParentId { get; init; }
        public Guid? SlotId { get; init; }
        public int ProjectId { get; init; }
        public string Title { get; init; } = string.Empty;
    }

    private static Dictionary<string, object?> Route(params (string Key, object? Value)[] values) =>
        values.ToDictionary(v => v.Key, v => v.Value);

    [Fact]
    public void Binds_AnIdFromTheRoute()
    {
        var id = Guid.NewGuid();
        var command = new Command();

        RouteIdBinder.Bind(command, Route(("id", id)));

        command.Id.ShouldBe(id);
    }

    [Fact]
    public void Matches_RouteValueNamesCaseInsensitively()
    {
        // The route declares {parentId}; the property is ParentId.
        var id = Guid.NewGuid();
        var command = new Command();

        RouteIdBinder.Bind(command, Route(("parentId", id)));

        command.ParentId.ShouldBe(id);
    }

    [Fact]
    public void Binds_EveryMatchingIdAtOnce()
    {
        var id = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var command = new Command();

        RouteIdBinder.Bind(command, Route(("id", id), ("parentId", parentId)));

        command.Id.ShouldBe(id);
        command.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void Overwrites_ABodyValueThatDisagreesWithTheRoute()
    {
        // The handler already overwrites from the route, so validating the body's value would mean
        // checking something that is about to be discarded.
        var routeId = Guid.NewGuid();
        var command = new Command { Id = Guid.NewGuid() };

        RouteIdBinder.Bind(command, Route(("id", routeId)));

        command.Id.ShouldBe(routeId);
    }

    [Fact]
    public void Leaves_PropertiesWithNoMatchingRouteValue()
    {
        var slotId = Guid.NewGuid();
        var command = new Command { SlotId = slotId };

        RouteIdBinder.Bind(command, Route(("parentId", Guid.NewGuid())));

        // SlotId is real data from the body, not addressing — the route says nothing about it.
        command.SlotId.ShouldBe(slotId);
    }

    [Fact]
    public void Ignores_NonGuidProperties()
    {
        var command = new Command { ProjectId = 42, Title = "Trial" };

        RouteIdBinder.Bind(command, Route(("projectId", 7), ("title", "Replaced")));

        command.ProjectId.ShouldBe(42);
        command.Title.ShouldBe("Trial");
    }

    [Fact]
    public void Ignores_ARouteValueThatIsNotAGuid()
    {
        var command = new Command();

        RouteIdBinder.Bind(command, Route(("id", "not-a-guid")));

        command.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Ignores_AnEmptyGuidInTheRoute()
    {
        var existing = Guid.NewGuid();
        var command = new Command { Id = existing };

        RouteIdBinder.Bind(command, Route(("id", Guid.Empty)));

        command.Id.ShouldBe(existing);
    }

    [Fact]
    public void Fills_ANullableIdFromTheRoute()
    {
        var slotId = Guid.NewGuid();
        var command = new Command();

        RouteIdBinder.Bind(command, Route(("slotId", slotId)));

        command.SlotId.ShouldBe(slotId);
    }

    [Fact]
    public void Does_NothingWithoutRouteValues()
    {
        var command = new Command { Id = Guid.NewGuid() };
        var before = command.Id;

        RouteIdBinder.Bind(command, Route());

        command.Id.ShouldBe(before);
    }

    [Fact]
    public void Accepts_ARouteValueSuppliedAsAString()
    {
        // Route values arrive as strings from the URL rather than as typed Guids.
        var id = Guid.NewGuid();
        var command = new Command();

        RouteIdBinder.Bind(command, Route(("id", id.ToString())));

        command.Id.ShouldBe(id);
    }
}
