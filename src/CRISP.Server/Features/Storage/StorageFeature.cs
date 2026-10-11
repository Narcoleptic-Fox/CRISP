using Asp.Versioning;

namespace CRISP.Server.Features.Storage;

public class StorageFeature : IFeature
{
    public IServiceCollection AddFeature(IServiceCollection services) => services;
    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        // Users
        var group = app.MapGroup(nameof(Storage))
            .WithTags(nameof(Storage));
        // v1 
        var v1 = group.HasApiVersion(new ApiVersion(1, 0));
        // Authorized Endpoints
        var v1Authorized = v1;
        DownloadEndpoint.Map(v1Authorized);
        UploadEndpoint.Map(v1Authorized);
        DeleteEndpoint.Map(v1Authorized);

        return app;
    }
}
