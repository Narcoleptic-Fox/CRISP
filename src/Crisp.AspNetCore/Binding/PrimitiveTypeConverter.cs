using Microsoft.AspNetCore.Http;

namespace Crisp.Binding;

/// <summary>
/// Handles conversion of primitive types and nullable variants.
/// </summary>
internal class PrimitiveTypeConverter : ITypeConverter
{
    public bool CanConvert(Type targetType)
    {
        // Handle nullable types
        Type? underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            targetType = underlyingType;
        }

        return targetType.IsPrimitive || 
               targetType == typeof(string) || 
               targetType == typeof(decimal) || 
               targetType == typeof(DateTime) || 
               targetType == typeof(DateOnly) || 
               targetType == typeof(TimeOnly) || 
               targetType == typeof(Guid);
    }

    public object? Convert(object? value, Type targetType)
    {
        if (value == null)
        {
            return GetDefaultValue(targetType);
        }

        // Handle nullable types
        Type? underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            targetType = underlyingType;
        }

        try
        {
            return System.Convert.ChangeType(value, targetType);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
        {
            throw new BadHttpRequestException($"Cannot convert '{value}' to type {targetType.Name}");
        }
    }

    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}