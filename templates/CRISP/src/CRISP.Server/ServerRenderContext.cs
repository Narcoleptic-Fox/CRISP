using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CRISP.Server;

public class ServerRenderContext : IRenderContext
{
    public bool IsClient => false;
    public bool IsServer => true;

    public bool IsPreRendering(IComponentRenderMode? renderMode)
    {
        return renderMode is null || renderMode is InteractiveServerRenderMode { Prerender: true };
    }
}
