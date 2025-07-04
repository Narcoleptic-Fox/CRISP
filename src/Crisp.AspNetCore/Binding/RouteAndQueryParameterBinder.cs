using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Crisp.Binding;

/// <summary>
/// Binds parameters from both route values and query parameters.
/// This is the primary binder used for query endpoints.
/// </summary>
internal class RouteAndQueryParameterBinder<T> : IParameterBinder<T>
{
    private readonly IInstanceCreator<T> _instanceCreator;

    public RouteAndQueryParameterBinder(IInstanceCreator<T> instanceCreator)
    {
        _instanceCreator = instanceCreator;
    }

    public T Bind(ParameterBindingContext context)
    {
        return _instanceCreator.CreateInstance(context);
    }
}

/// <summary>
/// Non-generic version for use with runtime type discovery.
/// </summary>
internal class RouteAndQueryParameterBinder : IParameterBinder
{
    public bool CanBind(Type targetType)
    {
        // Can bind any type that has constructors or settable properties
        return targetType.IsClass || targetType.IsValueType;
    }

    public object Bind(ParameterBindingContext context)
    {
        var constructorCreator = new ConstructorInstanceCreator(context.TargetType);
        return constructorCreator.CreateInstance(context);
    }
}