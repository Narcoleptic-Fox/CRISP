using Microsoft.AspNetCore.Http;

namespace Crisp.Binding;

/// <summary>
/// Handles conversion of boolean types with flexible string parsing.
/// </summary>
internal class BooleanTypeConverter : ITypeConverter
{
    public bool CanConvert(Type targetType)
    {
        // Handle nullable boolean
        Type? underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            targetType = underlyingType;
        }

        return targetType == typeof(bool);
    }

    public object? Convert(object? value, Type targetType)
    {
        if (value == null)
        {
            Type? underlyingType = Nullable.GetUnderlyingType(targetType);
            return underlyingType != null ? null : false;
        }

        if (value is string stringValue)
        {
            // Handle various string representations of boolean
            return stringValue.ToLowerInvariant() switch
            {
                "true" or "1" or "yes" or "on" => true,
                "false" or "0" or "no" or "off" => false,
                _ => throw new BadHttpRequestException($"Cannot convert '{stringValue}' to boolean. Valid values are: true, false, 1, 0, yes, no, on, off")
            };
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        // Try to convert other numeric types
        try
        {
            var numericValue = System.Convert.ToDouble(value);
            return numericValue != 0;
        }
        catch
        {
            throw new BadHttpRequestException($"Cannot convert '{value}' to boolean");
        }
    }
}