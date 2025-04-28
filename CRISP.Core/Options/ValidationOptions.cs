namespace CRISP.Core.Options;

/// <summary>
/// Options for configuring validation behavior.
/// </summary>
public class ValidationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to skip validation when no validators are registered.
    /// Default is true, which means no validation exception will be thrown if no validators exist.
    /// </summary>
    public bool SkipValidationIfNoValidatorsRegistered { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to throw an exception when validation fails.
    /// Default is true, which means a ValidationException will be thrown on validation failure.
    /// </summary>
    public bool ThrowExceptionOnValidationFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically validate child objects.
    /// Default is true, which means child objects will also be validated if they have validators.
    /// </summary>
    public bool ValidateChildObjects { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum depth for child object validation.
    /// Default is 5, to prevent circular references from causing stack overflows.
    /// </summary>
    public int MaxChildValidationDepth { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether to log validation failures at warning level.
    /// Default is true.
    /// </summary>
    public bool LogValidationFailures { get; set; } = true;
}