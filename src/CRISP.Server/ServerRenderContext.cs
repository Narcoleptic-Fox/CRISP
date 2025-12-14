using CRISP.Client;
using Microsoft.AspNetCore.Components;

namespace CRISP.Server
{
    public sealed class ServerRenderContext(IHttpContextAccessor contextAccessor) : IRenderContext
    {
        /// <inheritdoc/>
        public bool IsClient => false;

        /// <inheritdoc/>
        public bool IsServer => true;

        /// <inheritdoc/>
        public bool IsPreRendering(IComponentRenderMode? renderMode)
        {
            if (renderMode is null)
                return false;
            return !contextAccessor.HttpContext?.Response.HasStarted ?? false;
        }
    }
}
