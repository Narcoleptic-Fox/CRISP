namespace Crisp.Binding;

/// <summary>
/// Defines a contract for binding HTTP request parameters to specific objects.
/// </summary>
/// <typeparam name="T">The type to bind parameters to.</typeparam>
internal interface IParameterBinder<T>
{
    /// <summary>
    /// Binds parameters from the HTTP context to create an instance of type T.
    /// </summary>
    /// <param name="context">The parameter binding context.</param>
    /// <returns>An instance of T with bound parameters.</returns>
    T Bind(ParameterBindingContext context);
}

/// <summary>
/// Non-generic interface for parameter binding.
/// </summary>
internal interface IParameterBinder
{
    /// <summary>
    /// Determines if this binder can handle the specified type.
    /// </summary>
    bool CanBind(Type targetType);

    /// <summary>
    /// Binds parameters from the HTTP context to create an instance of the target type.
    /// </summary>
    /// <param name="context">The parameter binding context.</param>
    /// <returns>An instance of the target type with bound parameters.</returns>
    object Bind(ParameterBindingContext context);
}