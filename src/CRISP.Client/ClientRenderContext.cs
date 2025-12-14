using Microsoft.AspNetCore.Components;

namespace CRISP.Client
{
    public sealed class ClientRenderContext : IRenderContext
    {
        /// <inheritdoc/>
        public bool IsClient => true;

        /// <inheritdoc/>
        public bool IsServer => false;

        /// <inheritdoc/>
        public bool IsPreRendering(IComponentRenderMode? renderMode) => false;
    }
}
