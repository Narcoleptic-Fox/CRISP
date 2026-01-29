using Microsoft.AspNetCore.Components;

namespace CRISP.Server;

public interface IRenderContext
{
    /// <summary>
    /// Rendering from the Client project. Using HTTP request for connectivity.
    /// </summary>
    bool IsClient { get; }

    /// <summary>
    /// Rendering from the Server project. Using WebSockets for connectivity.
    /// </summary>
    bool IsServer { get; }

    /// <summary>
    /// Rendering from the Server project. Indicates if the response has started rendering.
    /// </summary>
    bool IsPreRendering(IComponentRenderMode? renderMode);
}
