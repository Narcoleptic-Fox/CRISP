namespace CRISP.Validation;

/// <summary>
/// Defines a validator for validating requests.
/// </summary>
/// <typeparam name="T">The type of request to validate.</typeparam>
public interface IValidator<T>
{
    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(T request);
}

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult : IValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public List<ValidationError> Errors { get; set; } = [];

    /// <summary>
    /// Gets the validation errors as a read-only list.
    /// </summary>
    IReadOnlyList<ValidationError> IValidationResult.Errors => Errors;

    /// <summary>
    /// Adds an error to the validation result.
    /// </summary>
    /// <param name="error">The error message to add.</param>
    public void AddError(ValidationError error)
    {
        if (error is not null)
        {
            Errors.Add(error);
        }
    }

    /// <summary>
    /// Adds multiple errors to the validation result.
    /// </summary>
    /// <param name="errors">The error messages to add.</param>
    public void AddErrors(IEnumerable<ValidationError> errors)
    {
        if (errors is not null)
        {
            Errors.AddRange(errors);
        }
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful validation result.</returns>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(params ValidationError[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    /// <summary>
    /// Creates a failed validation result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="propertyName">The name of the property with the error.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(string errorMessage, string? propertyName = null) =>
        Failure(new ValidationError(propertyName ?? string.Empty, errorMessage));

    /// <summary>
    /// Combines multiple validation results into a single result.
    /// </summary>
    /// <param name="validationResults">The validation results to combine.</param>
    /// <returns>
    /// A new validation result that is valid only if all input results are valid,
    /// and contains all errors from all input results.
    /// </returns>
    public static ValidationResult Combine(IEnumerable<ValidationResult> validationResults)
    {
        if (validationResults == null)
        {
            return Success();
        }

        ValidationResult combinedResult = new();

        foreach (ValidationResult result in validationResults)
        {
            if (result != null && result.Errors.Any())
            {
                combinedResult.AddErrors(result.Errors);
            }
        }

        return combinedResult;
    }
}

/// <summary>
/// Represents a validation error.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property with the error.</param>
    /// <param name="errorMessage">The error message.</param>
    public ValidationError(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the name of the property with the error.
    /// </summary>
    public string PropertyName { get; set; }
}