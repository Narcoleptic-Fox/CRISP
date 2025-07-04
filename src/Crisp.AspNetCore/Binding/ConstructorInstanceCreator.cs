using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Crisp.Binding;

/// <summary>
/// Creates instances using constructor parameter binding.
/// This is the primary strategy for record types and classes with primary constructors.
/// </summary>
internal class ConstructorInstanceCreator<T> : IInstanceCreator<T>
{
    public T CreateInstance(ParameterBindingContext context)
    {
        Type type = typeof(T);
        
        // Try to create instance using default constructor first
        try
        {
            var defaultInstance = (T?)Activator.CreateInstance(type);
            if (defaultInstance != null)
            {
                // For classes with settable properties, bind values after creation
                if (type.IsClass && !type.IsSealed)
                {
                    BindPropertiesToInstance(defaultInstance, context);
                }
                return defaultInstance;
            }
        }
        catch (MissingMethodException)
        {
            // Handle records/classes without parameterless constructor
        }

        // Create using constructor parameters
        return CreateInstanceFromConstructor<T>(context);
    }

    private static T CreateInstanceFromConstructor<T>(ParameterBindingContext context)
    {
        Type type = typeof(T);
        var constructors = type.GetConstructors();
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
        
        if (constructor == null)
        {
            throw new InvalidOperationException($"No suitable constructor found for {type.Name}");
        }

        var parameters = constructor.GetParameters();
        var args = new object?[parameters.Length];
        var allValues = context.AllValues;

        // Map constructor parameters
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            if (allValues.TryGetValue(param.Name!, out var value))
            {
                try
                {
                    args[i] = context.TypeConverter.Convert(value, param.ParameterType);
                }
                catch (BadHttpRequestException)
                {
                    throw new BadHttpRequestException($"Invalid value for parameter '{param.Name}': {value}");
                }
            }
            else if (param.HasDefaultValue)
            {
                args[i] = param.DefaultValue;
            }
            else if (!param.ParameterType.IsValueType || Nullable.GetUnderlyingType(param.ParameterType) != null)
            {
                // For reference types and nullable value types, null is acceptable
                args[i] = null;
            }
            else
            {
                // Required value type parameter is missing
                throw new BadHttpRequestException($"Missing required parameter: {param.Name}");
            }
        }

        return (T)constructor.Invoke(args);
    }

    private static void BindPropertiesToInstance<T>(T instance, ParameterBindingContext context)
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
                catch (BadHttpRequestException)
                {
                    // Skip invalid property values
                }
            }
        }
    }
}

/// <summary>
/// Non-generic version for use with runtime type discovery.
/// </summary>
internal class ConstructorInstanceCreator : IInstanceCreator
{
    private readonly Type _targetType;

    public ConstructorInstanceCreator(Type targetType)
    {
        _targetType = targetType;
    }

    public bool CanCreate(Type targetType)
    {
        return targetType.IsClass || targetType.IsValueType;
    }

    public object CreateInstance(ParameterBindingContext context)
    {
        // Try to create instance using default constructor first
        try
        {
            var defaultInstance = Activator.CreateInstance(_targetType);
            if (defaultInstance != null)
            {
                // For classes with settable properties, bind values after creation
                if (_targetType.IsClass && !_targetType.IsSealed)
                {
                    BindPropertiesToInstance(defaultInstance, context);
                }
                return defaultInstance;
            }
        }
        catch (MissingMethodException)
        {
            // Handle records/classes without parameterless constructor
        }

        // Create using constructor parameters
        return CreateInstanceFromConstructor(context);
    }

    private object CreateInstanceFromConstructor(ParameterBindingContext context)
    {
        var constructors = _targetType.GetConstructors();
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
        
        if (constructor == null)
        {
            throw new InvalidOperationException($"No suitable constructor found for {_targetType.Name}");
        }

        var parameters = constructor.GetParameters();
        var args = new object?[parameters.Length];
        var allValues = context.AllValues;

        // Map constructor parameters
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            if (allValues.TryGetValue(param.Name!, out var value))
            {
                try
                {
                    args[i] = context.TypeConverter.Convert(value, param.ParameterType);
                }
                catch (BadHttpRequestException)
                {
                    throw new BadHttpRequestException($"Invalid value for parameter '{param.Name}': {value}");
                }
            }
            else if (param.HasDefaultValue)
            {
                args[i] = param.DefaultValue;
            }
            else if (!param.ParameterType.IsValueType || Nullable.GetUnderlyingType(param.ParameterType) != null)
            {
                // For reference types and nullable value types, null is acceptable
                args[i] = null;
            }
            else
            {
                // Required value type parameter is missing
                throw new BadHttpRequestException($"Missing required parameter: {param.Name}");
            }
        }

        return constructor.Invoke(args);
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
                catch (BadHttpRequestException)
                {
                    // Skip invalid property values
                }
            }
        }
    }
}