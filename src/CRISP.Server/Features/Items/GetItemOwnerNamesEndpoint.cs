using CRISP.Core.Common;
using CRISP.Core.Items;
using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//#if (IncludeAuth)
using CRISP.Server.Authorization;
//#endif

namespace CRISP.Server.Features.Items;

/// <summary>
/// Maps and handles the endpoint that returns distinct item owner names for autocomplete.
/// </summary>
public static class GetItemOwnerNamesEndpoint
{
    /// <summary>
    /// Maps the get item owner names endpoint to the item route group.
    /// </summary>
    /// <param name="group">The route group to configure.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapGet("/owners", HandleAsync)
        .WithName(nameof(GetItemOwnerNames))
//#if (IncludeAuth)
        .RequireAuthorization(Policies.ReadAccess)
//#endif
        .Produces<IEnumerable<AutoComplete>>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    /// <summary>
    /// Handles a request for distinct item owner names.
    /// </summary>
    /// <param name="query">The owner names query.</param>
    /// <param name="service">The owner names query service.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An OK result containing matching owner autocomplete models.</returns>
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetItemOwnerNames query,
        [FromServices] IQueryService<GetItemOwnerNames, IEnumerable<AutoComplete>> service,
        CancellationToken cancellationToken)
    {
        var owners = await service.Send(query, cancellationToken);
        return Results.Ok(owners);
    }
}

/// <summary>
/// Returns distinct, non-empty owner names sourced from existing items. Demonstrates
/// the "autocomplete sourced from existing values" pattern for free-text fields.
/// </summary>
internal sealed class GetItemOwnerNamesService(ApplicationDbContext db) : IQueryService<GetItemOwnerNames, IEnumerable<AutoComplete>>
{
    private readonly DbSet<ItemEntity> _items = db.Set<ItemEntity>();

    /// <summary>
    /// Sends a distinct owner names query.
    /// </summary>
    /// <param name="query">The owner names query.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A list of autocomplete models, one per distinct owner name.</returns>
    public async ValueTask<IEnumerable<AutoComplete>> Send(GetItemOwnerNames query, CancellationToken cancellationToken = default)
    {
        var owners = await _items
            .Where(i => i.Owner != null && i.Owner != "")
            .Where(i => query.SearchTerm == null || i.Owner!.Contains(query.SearchTerm))
            .Select(i => i.Owner!)
            .Distinct()
            .OrderBy(o => o)
            .ToListAsync(cancellationToken);

        // Owner is free text rather than an entity, so there is no backing identifier;
        // the client keys these suggestions by name.
        return [.. owners.Select(o => new AutoComplete { Id = Guid.Empty, Name = o })];
    }
}
