using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Crisp.Binding;

/// <summary>
/// Binds parameters from the request body (typically JSON).
/// This is the primary binder used for command endpoints.
/// </summary>
internal class BodyParameterBinder<T> : IParameterBinder<T>
{
    public async Task<T> BindAsync(ParameterBindingContext context)
    {
        var request = context.HttpContext.Request;
        
        if (request.ContentLength == 0 || request.Body == null)
        {
            throw new BadHttpRequestException("Request body is required");
        }

        try
        {
            var result = await JsonSerializer.DeserializeAsync<T>(
                request.Body, 
                JsonSerializerOptions.Default,
                context.HttpContext.RequestAborted);
            
            if (result == null)
            {
                throw new BadHttpRequestException("Request body cannot be null");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new BadHttpRequestException($"Invalid JSON in request body: {ex.Message}");
        }
    }

    public T Bind(ParameterBindingContext context)
    {
        // For synchronous binding, we need to handle this differently
        // In practice, command endpoints should use async binding
        return BindAsync(context).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Non-generic version for use with runtime type discovery.
/// </summary>
internal class BodyParameterBinder : IParameterBinder
{
    public bool CanBind(Type targetType)
    {
        // Can bind any type that can be deserialized from JSON
        return true;
    }

    public object Bind(ParameterBindingContext context)
    {
        var request = context.HttpContext.Request;
        
        if (request.ContentLength == 0 || request.Body == null)
        {
            throw new BadHttpRequestException("Request body is required");
        }

        try
        {
            var result = JsonSerializer.Deserialize(
                request.Body, 
                context.TargetType,
                JsonSerializerOptions.Default);
            
            if (result == null)
            {
                throw new BadHttpRequestException("Request body cannot be null");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new BadHttpRequestException($"Invalid JSON in request body: {ex.Message}");
        }
    }
}