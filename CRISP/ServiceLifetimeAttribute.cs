using Microsoft.Extensions.DependencyInjection;

namespace CRISP
{
    /// <summary>
    /// Specifies the lifetime of a service when it is registered with dependency injection.
    /// If not specified, services will default to Transient lifetime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ServiceLifetimeAttribute : Attribute
    {
        /// <summary>
        /// Gets the service lifetime.
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLifetimeAttribute"/> class.
        /// </summary>
        /// <param name="lifetime">The service lifetime to use.</param>
        public ServiceLifetimeAttribute(ServiceLifetime lifetime) => Lifetime = lifetime;
    }
}