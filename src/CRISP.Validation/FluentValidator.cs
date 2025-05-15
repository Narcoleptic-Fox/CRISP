using System.Linq.Expressions;

namespace CRISP.Validation;

/// <summary>
/// Base class for validators that use a fluent interface for defining validation rules.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
public abstract class FluentValidator<T> : IValidator<T>
{
    private readonly List<IValidationRule<T>> _rules = [];
    private readonly Dictionary<string, List<IValidationRule<T>>> _ruleSets = [];

    /// <summary>
    /// Constructor for FluentValidator.
    /// </summary>
    protected FluentValidator() => ConfigureValidationRules();

    /// <summary>
    /// Configure the validation rules for this validator.
    /// This method should be overridden in derived classes to define validation rules.
    /// </summary>
    protected abstract void ConfigureValidationRules();

    /// <summary>
    /// Creates a validation rule for a specific property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="expression">An expression that identifies the property being validated (e.g., x => x.Name).</param>
    /// <returns>A rule builder that can be used to chain validation rules.</returns>
    protected IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        string propertyName = GetPropertyName(expression);
        Func<T, TProperty> propertyFunc = expression.Compile();

        ValidationRule<T, TProperty> rule = new(propertyName, propertyFunc);

        if (_currentRuleSet != null)
        {
            if (!_ruleSets.TryGetValue(_currentRuleSet, out List<IValidationRule<T>>? ruleSet))
            {
                ruleSet = [];
                _ruleSets[_currentRuleSet] = ruleSet;
            }

            ruleSet.Add(rule);
        }
        else
        {
            _rules.Add(rule);
        }

        return new RuleBuilder<T, TProperty>(rule);
    }

    private string? _currentRuleSet;

    /// <summary>
    /// Creates a rule set for grouping validation rules.
    /// </summary>
    /// <param name="ruleSetName">The name of the rule set.</param>
    /// <param name="action">An action to configure rules within the rule set.</param>
    protected void RuleSet(string ruleSetName, Action action)
    {
        if (string.IsNullOrEmpty(ruleSetName))
            throw new ArgumentException("Rule set name cannot be null or empty", nameof(ruleSetName));

        if (action == null)
            throw new ArgumentNullException(nameof(action));

        string? oldRuleSet = _currentRuleSet;
        _currentRuleSet = ruleSetName;

        try
        {
            action();
        }
        finally
        {
            _currentRuleSet = oldRuleSet;
        }
    }

    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A validation result.</returns>
    public ValidationResult Validate(T instance)
    {
        List<ValidationError> errors = [];

        foreach (IValidationRule<T> rule in _rules)
        {
            errors.AddRange(rule.Validate(instance));
        }

        return errors.Count > 0
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates the specified instance using only the rules in the specified rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="ruleSet">The name of the rule set to validate against.</param>
    /// <returns>A validation result.</returns>
    public ValidationResult Validate(T instance, string ruleSet)
    {
        if (string.IsNullOrEmpty(ruleSet))
            return Validate(instance);

        if (!_ruleSets.TryGetValue(ruleSet, out List<IValidationRule<T>>? rules))
            return ValidationResult.Success(); // If ruleset doesn't exist, there are no errors

        List<ValidationError> errors = [];

        foreach (IValidationRule<T> rule in rules)
        {
            errors.AddRange(rule.Validate(instance));
        }

        return errors.Count > 0
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression) =>
    expression.Body is MemberExpression memberExpression ?
        GetMemberExpressionPath(memberExpression) :
        string.Empty;

    private static string GetMemberExpressionPath(MemberExpression memberExpression) =>
        // If the member expression is nested (e.g., x => x.Address.City), we need to build the full path
        memberExpression.Expression is MemberExpression nestedMember
            ? $"{GetMemberExpressionPath(nestedMember)}.{memberExpression.Member.Name}"
            : memberExpression.Member.Name;
}

/// <summary>
/// Interface for validation rules.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
public interface IValidationRule<T>
{
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>Collection of validation errors if any.</returns>
    IEnumerable<ValidationError> Validate(T instance);
}

/// <summary>
/// Validation rule for a specific property.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
public class ValidationRule<T, TProperty> : IValidationRule<T>
{
    internal readonly string _propertyName;
    internal readonly Func<T, TProperty> _propertyFunc;
    private readonly List<Func<T, TProperty, ValidationError?>> _validators = [];
    private readonly List<Func<T, TProperty, IEnumerable<ValidationError>>> _complexValidators = [];
    private Func<T, bool>? _condition;

    /// <summary>
    /// Constructor for ValidationRule.
    /// </summary>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="propertyFunc">Function to get the property value.</param>
    public ValidationRule(string propertyName, Func<T, TProperty> propertyFunc)
    {
        _propertyName = propertyName;
        _propertyFunc = propertyFunc;
    }

    /// <summary>
    /// Sets a condition for when this rule should be applied.
    /// </summary>
    /// <param name="condition">Function that returns true if validation should be applied.</param>
    public void SetCondition(Func<T, bool> condition) => _condition = condition;

    /// <summary>
    /// Adds a validator function to the rule.
    /// </summary>
    /// <param name="validator">A function that returns a ValidationError if validation fails, or null if validation passes.</param>
    internal void AddValidator(Func<T, TProperty, ValidationError?> validator) => _validators.Add(validator);

    /// <summary>
    /// Adds a complex validator function to the rule.
    /// </summary>
    /// <param name="validator">A function that returns multiple ValidationError objects if validation fails, or an empty collection if validation passes.</param>
    internal void AddComplexValidator(Func<T, TProperty, IEnumerable<ValidationError>> validator) => _complexValidators.Add(validator);

    /// <summary>
    /// Validates the specified instance against all validators in the rule.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>Collection of validation errors if any.</returns>
    public IEnumerable<ValidationError> Validate(T instance)
    {
        // Skip validation if condition is not met
        if (_condition != null && !_condition(instance))
        {
            return Array.Empty<ValidationError>();
        }

        List<ValidationError> errors = [];
        TProperty propertyValue = _propertyFunc(instance);

        foreach (Func<T, TProperty, ValidationError?> validator in _validators)
        {
            ValidationError? error = validator(instance, propertyValue);
            if (error != null)
            {
                errors.Add(error);
            }
        }

        foreach (Func<T, TProperty, IEnumerable<ValidationError>> complexValidator in _complexValidators)
        {
            errors.AddRange(complexValidator(instance, propertyValue));
        }

        return errors;
    }
}

/// <summary>
/// Interface for rule builders that allow chaining validation rules.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
public interface IRuleBuilder<T, TProperty>
{
    /// <summary>
    /// Specifies a custom error message to use when validation fails.
    /// </summary>
    /// <param name="errorMessage">The error message to use.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    IRuleBuilder<T, TProperty> WithMessage(string errorMessage);

    /// <summary>
    /// Specifies a condition that must be met for this rule to be applied.
    /// </summary>
    /// <param name="predicate">A function that returns true if the rule should be applied, or false if it should be ignored.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    IRuleBuilder<T, TProperty> When(Func<T, bool> predicate);

    /// <summary>
    /// Specifies a condition that must not be met for this rule to be applied.
    /// </summary>
    /// <param name="predicate">A function that returns true if the rule should be ignored, or false if it should be applied.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    IRuleBuilder<T, TProperty> Unless(Func<T, bool> predicate);
}

/// <summary>
/// Implementation of IRuleBuilder that allows chaining validation rules.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
public class RuleBuilder<T, TProperty> : IRuleBuilder<T, TProperty>
{
    private readonly ValidationRule<T, TProperty> _rule;
    private string? _customErrorMessage;
    private ValidatorDefinition? _lastDefinition;
    /// <summary>
    /// Constructor for RuleBuilder.
    /// </summary>
    /// <param name="rule">The validation rule to build upon.</param>
    public RuleBuilder(ValidationRule<T, TProperty> rule) => _rule = rule;

    /// <summary>
    /// Gets the underlying validation rule.
    /// </summary>
    /// <returns>The validation rule.</returns>
    internal ValidationRule<T, TProperty> GetRule() => _rule;

    /// <summary>
    /// Specifies a custom error message to use when validation fails.
    /// </summary>
    /// <param name="errorMessage">The error message to use.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public IRuleBuilder<T, TProperty> WithMessage(string errorMessage)
    {
        if (_lastDefinition != null)
        {
            _lastDefinition.CustomErrorMessage = errorMessage;
        }
        else
        {
            _customErrorMessage = errorMessage;
        }
        _customErrorMessage = errorMessage;
        return this;
    }

    /// <summary>
    /// Specifies a condition that must be met for this rule to be applied.
    /// </summary>
    /// <param name="predicate">A function that returns true if the rule should be applied, or false if it should be ignored.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public IRuleBuilder<T, TProperty> When(Func<T, bool> predicate)
    {
        _rule.SetCondition(predicate);
        return this;
    }

    /// <summary>
    /// Specifies a condition that must not be met for this rule to be applied.
    /// </summary>
    /// <param name="predicate">A function that returns true if the rule should be ignored, or false if it should be applied.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    public IRuleBuilder<T, TProperty> Unless(Func<T, bool> predicate)
    {
        _rule.SetCondition(x => !predicate(x));
        return this;
    }

    /// <summary>
    /// Adds a validation function to the rule.
    /// </summary>
    /// <param name="predicate">A function that returns true if the validation passes, or false if it fails.</param>
    /// <param name="errorMessageProvider">A function that provides the error message if validation fails.</param>
    /// <returns>The same rule builder, for chaining.</returns>
    internal IRuleBuilder<T, TProperty> AddValidator(
        Func<TProperty, bool> predicate,
        Func<string> errorMessageProvider)
    {
        // Create a validator definition that binds the predicate with its error message
        ValidatorDefinition validator = new(predicate, errorMessageProvider, _customErrorMessage);
        _lastDefinition = validator;
        _rule.AddValidator((instance, propertyValue) =>
        {
            if (!validator.Predicate(propertyValue))
            {
                string errorMessage = validator.CustomErrorMessage ?? validator.ErrorMessageProvider();

#if DEBUG
                Console.WriteLine($"ValidationError: PropertyName='{_rule._propertyName}', ErrorMessage='{errorMessage}'");
                Console.WriteLine($"CapturedErrorMessage: '{validator.CustomErrorMessage}'");
                Console.WriteLine($"ErrorMessageProvider: '{validator.ErrorMessageProvider()}'");
#endif

                return new ValidationError(_rule._propertyName, errorMessage);
            }
            return null;
        });

        // Reset the custom error message after creating the validator
        _customErrorMessage = null;
        return this;
    }

    internal interface IValidatorDefinition
    {
        Func<TProperty, bool> Predicate { get; }
        Func<string> ErrorMessageProvider { get; }
        string? CustomErrorMessage { get; }

    }
    /// <summary>
    /// Record that encapsulates a validator definition with its predicate and error message provider.
    /// </summary>
    internal record ValidatorDefinition : IValidatorDefinition
    {
        /// <summary>
        /// The validation predicate function.
        /// </summary>
        public Func<TProperty, bool> Predicate { get; }

        /// <summary>
        /// Function that provides the default error message.
        /// </summary>
        public Func<string> ErrorMessageProvider { get; }

        /// <summary>
        /// Optional custom error message that overrides the default error message.
        /// </summary>
        public string? CustomErrorMessage { get; set; }

        /// <summary>
        /// Creates a new validator definition.
        /// </summary>
        public ValidatorDefinition(Func<TProperty, bool> predicate, Func<string> errorMessageProvider, string? customErrorMessage)
        {
            Predicate = predicate;
            ErrorMessageProvider = errorMessageProvider;
            CustomErrorMessage = customErrorMessage;
        }
    }
}