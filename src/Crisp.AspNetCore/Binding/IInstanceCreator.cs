namespace Crisp.Binding;

/// <summary>
/// Defines a contract for creating instances with parameter binding.
/// </summary>
/// <typeparam name="T">The type to create instances of.</typeparam>
internal interface IInstanceCreator<T>
{
    /// <summary>
    /// Creates an instance of type T using the provided binding context.
    /// </summary>
    /// <param name="context">The parameter binding context containing values and type converter.</param>
    /// <returns>A new instance of type T with bound parameters.</returns>
    T CreateInstance(ParameterBindingContext context);
}

/// <summary>
/// Non-generic interface for instance creation.
/// </summary>
internal interface IInstanceCreator
{
    /// <summary>
    /// Determines if this creator can handle the specified type.
    /// </summary>
    bool CanCreate(Type targetType);

    /// <summary>
    /// Creates an instance of the target type using the provided binding context.
    /// </summary>
    /// <param name="context">The parameter binding context containing values and type converter.</param>
    /// <returns>A new instance of the target type with bound parameters.</returns>
    object CreateInstance(ParameterBindingContext context);
}