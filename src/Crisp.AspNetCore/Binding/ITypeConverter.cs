using Microsoft.AspNetCore.Http;

namespace Crisp.Binding;

/// <summary>
/// Defines a contract for converting values to specific types during parameter binding.
/// </summary>
internal interface ITypeConverter
{
    /// <summary>
    /// Determines if this converter can handle the specified target type.
    /// </summary>
    bool CanConvert(Type targetType);

    /// <summary>
    /// Converts the input value to the specified target type.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type to convert to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="BadHttpRequestException">Thrown when the conversion fails.</exception>
    object? Convert(object? value, Type targetType);
}