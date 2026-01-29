using FluentValidation.Results;

namespace CRISP.Core.Common
{
    /// <summary>
    /// Provides a base implementation for FluentValidation validators with enhanced value validation capabilities.
    /// </summary>
    /// <typeparam name="T">The type of object being validated. Must be a reference type.</typeparam>
    /// <remarks>
    /// This abstract class extends FluentValidation's <see cref="AbstractValidator{T}"/> to provide
    /// additional functionality for validating individual property values. This is particularly useful
    /// in scenarios where you need to validate single properties in real-time, such as during
    /// form input validation in web applications or incremental validation during data entry.
    /// </remarks>
    public abstract class BaseValidator<T> : AbstractValidator<T>
        where T : class
    {
        /// <summary>
        /// Gets a function that validates a specific property value of a model instance.
        /// </summary>
        /// <value>
        /// A function that takes a model instance and property name, and returns an asynchronous enumerable
        /// of validation error messages for that specific property. If validation passes, an empty collection is returned.
        /// </value>
        /// <remarks>
        /// This property provides a convenient way to validate individual properties without running
        /// the full validation suite. It's particularly useful for real-time validation scenarios
        /// where you want to provide immediate feedback as users interact with form fields.
        /// 
        /// The function creates a validation context that includes only the specified property,
        /// runs the validation, and returns any error messages specific to that property.
        /// </remarks>
        /// <example>
        /// <code>
        /// var validator = new PersonValidator();
        /// var person = new Person { Name = "", Email = "invalid-email" };
        /// var emailErrors = await validator.ValidateValue(person, "Email");
        /// // Returns validation errors for the Email property only
        /// </code>
        /// </example>
        public Func<T, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var validationContext = ValidationContext<T>.CreateWithOptions(model, options => options.IncludeProperties(propertyName));
            ValidationResult result = await ValidateAsync(validationContext).ConfigureAwait(false);
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
