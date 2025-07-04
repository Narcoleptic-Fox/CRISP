using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Crisp.Binding;

/// <summary>
/// Creates instances using property-based binding.
/// This is used for classes with parameterless constructors and settable properties.
/// </summary>
internal class PropertyInstanceCreator<T> : IInstanceCreator<T>
{
    public T CreateInstance(ParameterBindingContext context)
    {
        Type type = typeof(T);
        
        // Create instance using parameterless constructor
        var instance = (T?)Activator.CreateInstance(type);
        if (instance == null)
        {
            throw new InvalidOperationException($"Could not create instance of type {type.Name}");
        }

        // Bind properties from available values
        BindPropertiesToInstance(instance, context);
        
        return instance;
    }

    private static void BindPropertiesToInstance(T instance, ParameterBindingContext context)
    {
        Type type = typeof(T);
        var allValues = context.AllValues;

        foreach (var kvp in allValues)
        {
            PropertyInfo? property = type.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property?.CanWrite == true)
            {
                try
                {
                    object? convertedValue = context.TypeConverter.Convert(kvp.Value, property.PropertyType);
                    property.SetValue(instance, convertedValue);
                }
                catch (BadHttpRequestException ex)
                {
                    throw new BadHttpRequestException($"Cannot bind property '{property.Name}': {ex.Message}");
                }
            }
        }
    }
}

/// <summary>
/// Non-generic version for use with runtime type discovery.
/// </summary>
internal class PropertyInstanceCreator : IInstanceCreator
{
    private readonly Type _targetType;

    public PropertyInstanceCreator(Type targetType)
    {
        _targetType = targetType;
    }

    public bool CanCreate(Type targetType)
    {
        // Can create if type has parameterless constructor and settable properties
        return targetType.GetConstructor(Type.EmptyTypes) != null &&
               targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Any(p => p.CanWrite);
    }

    public object CreateInstance(ParameterBindingContext context)
    {
        // Create instance using parameterless constructor
        var instance = Activator.CreateInstance(_targetType);
        if (instance == null)
        {
            throw new InvalidOperationException($"Could not create instance of type {_targetType.Name}");
        }

        // Bind properties from available values
        BindPropertiesToInstance(instance, context);
        
        return instance;
    }

    private void BindPropertiesToInstance(object instance, ParameterBindingContext context)
    {
        var allValues = context.AllValues;

        foreach (var kvp in allValues)
        {
            PropertyInfo? property = _targetType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property?.CanWrite == true)
            {
                try
                {
                    object? convertedValue = context.TypeConverter.Convert(kvp.Value, property.PropertyType);
                    property.SetValue(instance, convertedValue);
                }
                catch (BadHttpRequestException ex)
                {
                    throw new BadHttpRequestException($"Cannot bind property '{property.Name}': {ex.Message}");
                }
            }
        }
    }
}