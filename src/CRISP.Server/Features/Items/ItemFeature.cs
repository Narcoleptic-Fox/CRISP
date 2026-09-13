using CRISP.Core.Common;
using CRISP.Core.Items;
using CRISP.Server.Common;
using CRISP.Server.Data.Entities;
using CRISP.Server.Events;
using CRISP.Server.Features.Items.Events;
using Asp.Versioning;
using Humanizer;

namespace CRISP.Server.Features.Items;

/// <summary>
/// Registers item services and maps item endpoints. This is the worked example of a
/// full vertical slice; copy it when adding a new domain feature.
/// </summary>
public sealed class ItemFeature : IFeature
{
    /// <summary>
    /// Registers item command and query handlers and the example event handler.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public IServiceCollection AddFeature(IServiceCollection services)
    {
        services.AddScoped<ICreateService<CreateItem>, CreateItemService>();
        services.AddScoped<IModifyService<UpdateItem>, UpdateItemService>();
        services.AddScoped<IQueryService<GetItem, Item>, GetItemService>();
        services.AddScoped<IQueryService<GetItems, PagedResponse<Item>>, GetItemsService>();
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteItemService>(nameof(Items));
        services.AddScoped<IQueryService<GetItemOwnerNames, IEnumerable<AutoComplete>>, GetItemOwnerNamesService>();

        // Example in-process event handler; see ItemRenamedLogger.
        services.AddScoped<IEventHandler<ItemRenamedEvent>, ItemRenamedLogger>();

        return services;
    }

    /// <summary>
    /// Maps versioned item endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder to configure.</param>
    /// <returns>The endpoint route builder.</returns>
    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(nameof(Item).Pluralize())
            .WithTags(nameof(Item));

        var v1 = group.HasApiVersion(new ApiVersion(1, 0));
        var v1Authorized = v1;
//#if (IncludeAuth)
        v1Authorized = v1Authorized.RequireAuthorization();
//#endif
        CreateItemEndpoint.Map(v1Authorized);
        UpdateItemEndpoint.Map(v1Authorized);
        DeleteItemEndpoint.Map(v1Authorized);
        GetItemEndpoint.Map(v1Authorized);
        GetItemsEndpoint.Map(v1Authorized);
        GetItemOwnerNamesEndpoint.Map(v1Authorized);
        GetActivityEndpoint.Map<ItemEntity>(v1Authorized);

        return app;
    }
}
