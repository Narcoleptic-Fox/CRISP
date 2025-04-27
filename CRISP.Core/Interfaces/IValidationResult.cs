using CRISP.Core.Behaviors;

namespace CRISP.Core.Interfaces;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public interface IValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    bool IsValid { get; }
    
    /// <summary>
    /// Gets the validation errors, if any.
    /// </summary>
    IReadOnlyList<ValidationError> Errors { get; }
}