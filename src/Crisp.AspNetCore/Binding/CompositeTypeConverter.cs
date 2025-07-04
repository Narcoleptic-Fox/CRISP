using Microsoft.AspNetCore.Http;

namespace Crisp.Binding;

/// <summary>
/// Orchestrates multiple type converters to handle various type conversion scenarios.
/// </summary>
internal class CompositeTypeConverter : ITypeConverter
{
    private readonly IReadOnlyList<ITypeConverter> _converters;

    public CompositeTypeConverter()
    {
        var primitiveConverter = new PrimitiveTypeConverter();
        var booleanConverter = new BooleanTypeConverter();
        
        _converters = new List<ITypeConverter>
        {
            booleanConverter,           // Check boolean first for better string handling
            new ArrayTypeConverter(primitiveConverter), // Arrays with primitive elements
            primitiveConverter          // Fallback to primitive conversion
        };
    }

    public bool CanConvert(Type targetType)
    {
        return _converters.Any(converter => converter.CanConvert(targetType));
    }

    public object? Convert(object? value, Type targetType)
    {
        foreach (var converter in _converters)
        {
            if (converter.CanConvert(targetType))
            {
                return converter.Convert(value, targetType);
            }
        }

        throw new BadHttpRequestException($"No converter available for type {targetType.Name}");
    }
}