using Microsoft.AspNetCore.Http;

namespace Crisp.Binding;

/// <summary>
/// Handles conversion of array types from string arrays or single string values.
/// </summary>
internal class ArrayTypeConverter : ITypeConverter
{
    private readonly ITypeConverter _elementConverter;

    public ArrayTypeConverter(ITypeConverter elementConverter)
    {
        _elementConverter = elementConverter;
    }

    public bool CanConvert(Type targetType)
    {
        return targetType.IsArray;
    }

    public object? Convert(object? value, Type targetType)
    {
        if (value == null)
        {
            return null;
        }

        if (!targetType.IsArray)
        {
            throw new ArgumentException($"Target type {targetType.Name} is not an array type");
        }

        Type elementType = targetType.GetElementType()!;

        if (value is string[] stringArray)
        {
            Array array = Array.CreateInstance(elementType, stringArray.Length);
            for (int i = 0; i < stringArray.Length; i++)
            {
                try
                {
                    var convertedElement = _elementConverter.Convert(stringArray[i], elementType);
                    array.SetValue(convertedElement, i);
                }
                catch (BadHttpRequestException)
                {
                    throw new BadHttpRequestException($"Cannot convert array element '{stringArray[i]}' to type {elementType.Name}");
                }
            }
            return array;
        }
        else if (value is string singleValue)
        {
            // Single value to array
            Array array = Array.CreateInstance(elementType, 1);
            try
            {
                var convertedElement = _elementConverter.Convert(singleValue, elementType);
                array.SetValue(convertedElement, 0);
            }
            catch (BadHttpRequestException)
            {
                throw new BadHttpRequestException($"Cannot convert value '{singleValue}' to array element type {elementType.Name}");
            }
            return array;
        }

        throw new BadHttpRequestException($"Cannot convert value of type {value.GetType().Name} to array type {targetType.Name}");
    }
}