using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CRISP.Validation;

/// <summary>
/// Extension methods for <see cref="IRuleBuilder{T,TProperty}"/> that add common validation rules.
/// </summary>
public static class ValidationRuleBuilderExtensions
{
    /// <summary>
    /// Specifies that the property value must not be null or empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, string> NotEmpty<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ((RuleBuilder<T, string>)ruleBuilder).AddValidator(
            value => !string.IsNullOrWhiteSpace(value),
            () => "Must not be empty.");
    }

    /// <summary>
    /// Specifies that the property value must not be null.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TProperty?> NotNull<T, TProperty>(this IRuleBuilder<T, TProperty?> ruleBuilder)
    {
        return ((RuleBuilder<T, TProperty?>)ruleBuilder).AddValidator(
            value => value != null,
            () => "Must not be null.");
    }

    /// <summary>
    /// Specifies that the collection must contain at least the specified number of items.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="min">The minimum number of items.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TCollection> MinCount<T, TCollection>(
        this IRuleBuilder<T, TCollection> ruleBuilder,
        int min)
        where TCollection : IEnumerable?
    {
        var actualRuleBuilder = (RuleBuilder<T, TCollection>)ruleBuilder;
        var rule = actualRuleBuilder.GetRule();

        return actualRuleBuilder.AddValidator(
            value =>
            {
                if (value == null)
                    return false;

                int count = 0;
                foreach (var _ in value)
                {
                    count++;
                    if (count >= min)
                        return true;
                }
                return false;
            },
            () => $"Must contain at least {min} item(s).");
    }

    /// <summary>
    /// Specifies that the collection must contain at most the specified number of items.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="max">The maximum number of items.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TCollection> MaxCount<T, TCollection>(
        this IRuleBuilder<T, TCollection> ruleBuilder,
        int max)
        where TCollection : IEnumerable
    {
        return ((RuleBuilder<T, TCollection>)ruleBuilder).AddValidator(
            value =>
            {
                if (value == null)
                    return true;

                int count = 0;
                foreach (var _ in value)
                {
                    count++;
                    if (count > max)
                        return false;
                }
                return true;
            },
            () => $"Must contain at most {max} item(s).");
    }

    /// <summary>
    /// Specifies that each item in the collection must satisfy the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TElement">The type of elements in the collection.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="predicate">A function that returns true if the element is valid, or false if it is invalid.</param>
    /// <param name="errorMessage">The error message to use if validation fails.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, IEnumerable<TElement>> ForEach<T, TElement>(
        this IRuleBuilder<T, IEnumerable<TElement>> ruleBuilder,
        Func<TElement, bool> predicate,
        string errorMessage)
    {
        return ((RuleBuilder<T, IEnumerable<TElement>>)ruleBuilder).AddValidator(
            value =>
            {
                if (value == null)
                    return true;

                return value.All(predicate);
            },
            () => errorMessage);
    }

    /// <summary>
    /// Specifies that each item in the List must satisfy the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TElement">The type of elements in the collection.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="predicate">A function that returns true if the element is valid, or false if it is invalid.</param>
    /// <param name="errorMessage">The error message to use if validation fails.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, List<TElement>> ForEach<T, TElement>(
        this IRuleBuilder<T, List<TElement>> ruleBuilder,
        Func<TElement, bool> predicate,
        string errorMessage)
    {
        var actualRuleBuilder = (RuleBuilder<T, List<TElement>>)ruleBuilder;
        var rule = actualRuleBuilder.GetRule();

        rule.AddValidator((instance, list) =>
        {
            if (list == null || list.All(predicate))
                return null;

            return new ValidationError(rule._propertyName, errorMessage);
        });

        return ruleBuilder;
    }

    /// <summary>
    /// Specifies that each item in the collection must be validated by the specified validator.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TElement">The type of elements in the collection.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="validator">The validator to use for each element.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, IEnumerable<TElement>> ForEach<T, TElement>(
        this IRuleBuilder<T, IEnumerable<TElement>> ruleBuilder,
        IValidator<TElement> validator)
    {
        // Need to use a special implementation for this validator since it returns multiple errors
        var actualRuleBuilder = (RuleBuilder<T, IEnumerable<TElement>>)ruleBuilder;
        var rule = actualRuleBuilder.GetRule();

        rule.AddComplexValidator((instance, collection) =>
        {
            if (collection == null)
                return Array.Empty<ValidationError>();

            List<ValidationError> allErrors = new();
            int index = 0;

            foreach (var item in collection)
            {
                var result = validator.Validate(item);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        string indexedPropertyName = string.IsNullOrEmpty(error.PropertyName)
                            ? $"{rule._propertyName}[{index}]"
                            : $"{rule._propertyName}[{index}].{error.PropertyName}";

                        allErrors.Add(new ValidationError(indexedPropertyName, error.ErrorMessage));
                    }
                }
                index++;
            }

            return allErrors;
        });

        return ruleBuilder;
    }

    /// <summary>
    /// Validates a single object using a nested validator.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="validator">The validator to use for the object.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TProperty> ForEach<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        IValidator<TProperty> validator) where TProperty : class
    {
        var actualRuleBuilder = (RuleBuilder<T, TProperty>)ruleBuilder;
        var rule = actualRuleBuilder.GetRule();

        rule.AddComplexValidator((instance, property) =>
        {
            if (property == null)
                return Array.Empty<ValidationError>();

            List<ValidationError> allErrors = new();
            var result = validator.Validate(property);

#if DEBUG
            Console.WriteLine($"ForEach validation for {rule._propertyName}: Found {result.Errors.Count} errors");
#endif
            
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    // Correctly construct nested property paths by combining parent and child property names
                    string nestedPropertyName = string.IsNullOrEmpty(error.PropertyName)
                        ? rule._propertyName
                        : $"{rule._propertyName}.{error.PropertyName}";

#if DEBUG
                    Console.WriteLine($"Creating nested error: PropertyName='{nestedPropertyName}', ErrorMessage='{error.ErrorMessage}' (Original: PropertyName='{error.PropertyName}')");
#endif
                    
                    allErrors.Add(new ValidationError(nestedPropertyName, error.ErrorMessage));
                }
            }

            return allErrors;
        });

        return ruleBuilder;
    }

    /// <summary>
    /// Specifies that the property value must be equal to the specified value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="valueToCompare">The value to compare.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TProperty> Equal<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty valueToCompare)
        where TProperty : IEquatable<TProperty>
    {
        return ((RuleBuilder<T, TProperty>)ruleBuilder).AddValidator(
            value => value != null && value.Equals(valueToCompare),
            () => $"Must be equal to '{valueToCompare}'.");
    }

    /// <summary>
    /// Specifies that the property value must have a length within the specified range.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="min">The minimum length.</param>
    /// <param name="max">The maximum length.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, string> Length<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int min,
        int max)
    {
        return ((RuleBuilder<T, string>)ruleBuilder).AddValidator(
            value => value != null && value.Length >= min && value.Length <= max,
            () => $"Length must be between {min} and {max}.");
    }

    /// <summary>
    /// Specifies that the property value must have a minimum length.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="min">The minimum length.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, string> MinLength<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int min)
    {
        return ((RuleBuilder<T, string>)ruleBuilder).AddValidator(
            value => value != null && value.Length >= min,
            () => $"Length must be at least {min}.");
    }

    /// <summary>
    /// Specifies that the property value must have a maximum length.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="max">The maximum length.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, string> MaxLength<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int max)
    {
        return ((RuleBuilder<T, string>)ruleBuilder).AddValidator(
            value => value == null || value.Length <= max,
            () => $"Length must not exceed {max}.");
    }

    /// <summary>
    /// Specifies that the property value must match the specified regular expression.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="regex">The regular expression.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, string> Matches<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        string regex)
    {
        return ((RuleBuilder<T, string>)ruleBuilder).AddValidator(
            value => value != null && Regex.IsMatch(value, regex),
            () => $"Must match the regular expression '{regex}'.");
    }

    /// <summary>
    /// A sophisticated email regex that complies with RFC 5322 standards.
    /// It validates:
    /// - Local part with allowed special characters
    /// - Domain with subdomains
    /// - Various TLDs
    /// - No consecutive dots
    /// - Properly quoted strings in local part
    /// - IP literals in domain part
    /// </summary>
    public static readonly string EmailRegex = @"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$";

    /// <summary>
    /// Specifies that the property value must be a valid email address.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, string> EmailAddress<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ((RuleBuilder<T, string>)ruleBuilder).AddValidator(
            value => value != null && Regex.IsMatch(value, EmailRegex),
            () => "Must be a valid email address.");
    }

    /// <summary>
    /// Specifies that the property value must be greater than the specified value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="valueToCompare">The value to compare.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TProperty> GreaterThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty valueToCompare)
        where TProperty : IComparable<TProperty>
    {
        var actualRuleBuilder = (RuleBuilder<T, TProperty>)ruleBuilder;
        var rule = actualRuleBuilder.GetRule();

        return actualRuleBuilder.AddValidator(
            value => value != null && value.CompareTo(valueToCompare) > 0,
            () => $"{rule._propertyName} must be greater than {valueToCompare}.");
    }

    /// <summary>
    /// Specifies that the property value must be less than the specified value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="valueToCompare">The value to compare.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TProperty> LessThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty valueToCompare)
        where TProperty : IComparable<TProperty>
    {
        return ((RuleBuilder<T, TProperty>)ruleBuilder).AddValidator(
            value => value != null && value.CompareTo(valueToCompare) < 0,
            () => $"Must be less than {valueToCompare}.");
    }

    /// <summary>
    /// Specifies a custom validation rule.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="predicate">A function that returns true if the validation passes, or false if it fails.</param>
    /// <param name="errorMessage">The error message to use if validation fails.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TProperty> Must<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<TProperty, bool> predicate,
        string errorMessage)
    {
        return ((RuleBuilder<T, TProperty>)ruleBuilder).AddValidator(
            predicate,
            () => errorMessage);
    }

    /// <summary>
    /// Specifies that the property must be in a specified collection.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to extend.</param>
    /// <param name="validValues">The collection of valid values.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public static IRuleBuilder<T, TProperty> In<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        params TProperty[] validValues)
        where TProperty : IEquatable<TProperty>
    {
        return ((RuleBuilder<T, TProperty>)ruleBuilder).AddValidator(
            value => Array.Exists(validValues, validValue => value != null && value.Equals(validValue)),
            () => $"Must be one of the allowed values: {string.Join(", ", validValues)}.");
    }
}