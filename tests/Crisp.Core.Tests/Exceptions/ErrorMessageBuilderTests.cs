using Crisp.Exceptions;

namespace Crisp.Core.Tests.Exceptions;

public class ErrorMessageBuilderTests
{
    #region NotFound Tests

    [Fact]
    public void NotFound_WithValidParameters_ReturnsFormattedMessage()
    {
        // Arrange
        string entityName = "User";
        object identifier = 123;

        // Act
        string result = ErrorMessageBuilder.NotFound(entityName, identifier);

        // Assert
        result.Should().Contain("User");
        result.Should().Contain("123");
        result.Should().Contain("was not found");
        result.Should().Contain("Suggestions:");
        result.Should().Contain("Verify the user ID is correct");
        result.Should().Contain("Check if the user still exists");
        result.Should().Contain("Ensure you have permission to access this resource");
    }

    [Fact]
    public void NotFound_WithStringIdentifier_HandlesCorrectly()
    {
        // Arrange
        string entityName = "Product";
        string identifier = "ABC-123";

        // Act
        string result = ErrorMessageBuilder.NotFound(entityName, identifier);

        // Assert
        result.Should().Contain("Product");
        result.Should().Contain("ABC-123");
        result.Should().Contain("was not found");
    }

    [Fact]
    public void NotFound_WithCustomSuggestions_IncludesCustomSuggestion()
    {
        // Arrange
        string entityName = "Order";
        object identifier = 456;
        string customSuggestion = "Check if the order was recently canceled";

        // Act
        string result = ErrorMessageBuilder.NotFound(entityName, identifier, customSuggestion);

        // Assert
        result.Should().Contain(customSuggestion);
        result.Should().Contain("Verify the order ID is correct");
        result.Should().Contain("Check if the order still exists");
    }

    [Fact]
    public void NotFound_WithNullSuggestions_OnlyIncludesStandardSuggestions()
    {
        // Arrange
        string entityName = "Customer";
        object identifier = 789;

        // Act
        string result = ErrorMessageBuilder.NotFound(entityName, identifier, null);

        // Assert
        result.Should().Contain("Verify the customer ID is correct");
        result.Should().Contain("Check if the customer still exists");
        result.Should().Contain("Ensure you have permission to access this resource");
    }

    [Fact]
    public void NotFound_WithEmptySuggestions_OnlyIncludesStandardSuggestions()
    {
        // Arrange
        string entityName = "Invoice";
        object identifier = "INV-001";

        // Act
        string result = ErrorMessageBuilder.NotFound(entityName, identifier, "");

        // Assert
        result.Should().Contain("Verify the invoice ID is correct");
        result.Should().Contain("Check if the invoice still exists");
        result.Should().Contain("Ensure you have permission to access this resource");
        result.Should().NotContain("• \n");
    }

    [Fact]
    public void NotFound_WithWhitespaceSuggestions_OnlyIncludesStandardSuggestions()
    {
        // Arrange
        string entityName = "Document";
        object identifier = 999;

        // Act
        string result = ErrorMessageBuilder.NotFound(entityName, identifier, "   ");

        // Assert
        result.Should().Contain("Verify the document ID is correct");
        result.Should().NotContain("•    ");
    }

    [Fact]
    public void NotFound_CaseHandling_ConvertsToLowerCase()
    {
        // Arrange
        string entityName = "PRODUCT";
        object identifier = 1;

        // Act
        string result = ErrorMessageBuilder.NotFound(entityName, identifier);

        // Assert
        result.Should().Contain("The PRODUCT with identifier");
        result.Should().Contain("Verify the product ID is correct");
        result.Should().Contain("Check if the product still exists");
    }

    #endregion

    #region Unauthorized Tests

    [Fact]
    public void Unauthorized_WithBasicParameters_ReturnsFormattedMessage()
    {
        // Arrange
        string operation = "delete";
        string resource = "user accounts";

        // Act
        string result = ErrorMessageBuilder.Unauthorized(operation, resource);

        // Assert
        result.Should().Contain("You are not authorized to delete user accounts");
        result.Should().Contain("Suggestions:");
        result.Should().Contain("Verify you have the necessary permissions for this operation");
        result.Should().Contain("Verify you are logged in with the correct account");
        result.Should().Contain("Check that your session has not expired");
        result.Should().Contain("Contact your administrator if you believe you should have access");
    }

    [Fact]
    public void Unauthorized_WithRequiredPermission_IncludesSpecificPermission()
    {
        // Arrange
        string operation = "read";
        string resource = "financial reports";
        string requiredPermission = "Finance.Read";

        // Act
        string result = ErrorMessageBuilder.Unauthorized(operation, resource, requiredPermission);

        // Assert
        result.Should().Contain("You are not authorized to read financial reports");
        result.Should().Contain("Ensure you have the 'Finance.Read' permission");
        result.Should().Contain("Verify you are logged in with the correct account");
        result.Should().Contain("Contact your administrator if you believe you should have access");
    }

    [Fact]
    public void Unauthorized_WithNullPermission_UsesGenericPermissionMessage()
    {
        // Arrange
        string operation = "update";
        string resource = "configuration";

        // Act
        string result = ErrorMessageBuilder.Unauthorized(operation, resource, null);

        // Assert
        result.Should().Contain("Verify you have the necessary permissions for this operation");
        result.Should().NotContain("Ensure you have the '' permission");
    }

    [Fact]
    public void Unauthorized_WithEmptyPermission_UsesGenericPermissionMessage()
    {
        // Arrange
        string operation = "create";
        string resource = "projects";

        // Act
        string result = ErrorMessageBuilder.Unauthorized(operation, resource, "");

        // Assert
        result.Should().Contain("Verify you have the necessary permissions for this operation");
        result.Should().NotContain("Ensure you have the '' permission");
    }

    [Fact]
    public void Unauthorized_WithWhitespacePermission_UsesGenericPermissionMessage()
    {
        // Arrange
        string operation = "export";
        string resource = "data";

        // Act
        string result = ErrorMessageBuilder.Unauthorized(operation, resource, "   ");

        // Assert
        result.Should().Contain("Verify you have the necessary permissions for this operation");
        result.Should().NotContain("Ensure you have the '   ' permission");
    }

    #endregion

    #region ValidationError Tests

    [Fact]
    public void ValidationError_WithAllParameters_ReturnsFormattedMessage()
    {
        // Arrange
        string fieldName = "Email";
        object value = "invalid-email";
        string constraint = "Must be a valid email address";
        string suggestion = "Use format: user@domain.com";

        // Act
        string result = ErrorMessageBuilder.ValidationError(fieldName, value, constraint, suggestion);

        // Assert
        result.Should().Contain("The field 'Email' has an invalid value 'invalid-email'");
        result.Should().Contain("Must be a valid email address");
        result.Should().Contain("Suggestion: Use format: user@domain.com");
    }

    [Fact]
    public void ValidationError_WithNullValue_HandlesNullCorrectly()
    {
        // Arrange
        string fieldName = "Name";
        object? value = null;
        string constraint = "Cannot be null or empty";

        // Act
        string result = ErrorMessageBuilder.ValidationError(fieldName, value, constraint);

        // Assert
        result.Should().Contain("The field 'Name' has an invalid value");
        result.Should().NotContain("''");
        result.Should().Contain("Cannot be null or empty");
        result.Should().NotContain("Suggestion:");
    }

    [Fact]
    public void ValidationError_WithoutSuggestion_OmitsSuggestionSection()
    {
        // Arrange
        string fieldName = "Age";
        object value = -5;
        string constraint = "Must be a positive number";

        // Act
        string result = ErrorMessageBuilder.ValidationError(fieldName, value, constraint, null);

        // Assert
        result.Should().Contain("The field 'Age' has an invalid value '-5'");
        result.Should().Contain("Must be a positive number");
        result.Should().NotContain("Suggestion:");
    }

    [Fact]
    public void ValidationError_WithEmptySuggestion_OmitsSuggestionSection()
    {
        // Arrange
        string fieldName = "Password";
        object value = "123";
        string constraint = "Must be at least 8 characters";

        // Act
        string result = ErrorMessageBuilder.ValidationError(fieldName, value, constraint, "");

        // Assert
        result.Should().Contain("The field 'Password' has an invalid value '123'");
        result.Should().Contain("Must be at least 8 characters");
        result.Should().NotContain("Suggestion:");
    }

    [Fact]
    public void ValidationError_WithWhitespaceSuggestion_OmitsSuggestionSection()
    {
        // Arrange
        string fieldName = "Username";
        object value = "user@";
        string constraint = "Cannot contain special characters";

        // Act
        string result = ErrorMessageBuilder.ValidationError(fieldName, value, constraint, "   ");

        // Assert
        result.Should().Contain("The field 'Username' has an invalid value 'user@'");
        result.Should().Contain("Cannot contain special characters");
        result.Should().NotContain("Suggestion:");
    }

    #endregion

    #region ConfigurationError Tests

    [Fact]
    public void ConfigurationError_WithValidParameters_ReturnsFormattedMessage()
    {
        // Arrange
        string component = "Database Connection";
        string issue = "Connection string is missing";
        string solution = "Add a valid connection string to appsettings.json";

        // Act
        string result = ErrorMessageBuilder.ConfigurationError(component, issue, solution);

        // Assert
        result.Should().Contain("Configuration error in Database Connection: Connection string is missing");
        result.Should().Contain("To resolve this issue:");
        result.Should().Contain("Add a valid connection string to appsettings.json");
        result.Should().Contain("Check your application configuration files");
        result.Should().Contain("Verify all required dependencies are registered");
        result.Should().Contain("Review the CRISP documentation for setup guidance");
    }

    [Fact]
    public void ConfigurationError_SolutionAppearsFirst_InSuggestionsList()
    {
        // Arrange
        string component = "Authentication";
        string issue = "JWT secret key not configured";
        string solution = "Set the JwtSettings:SecretKey in configuration";

        // Act
        string result = ErrorMessageBuilder.ConfigurationError(component, issue, solution);

        // Assert
        var lines = result.Split('\n');
        var solutionLineIndex = Array.FindIndex(lines, line => line.Contains(solution));
        var checkConfigLineIndex = Array.FindIndex(lines, line => line.Contains("Check your application configuration files"));
        
        solutionLineIndex.Should().BeLessThan(checkConfigLineIndex);
    }

    #endregion

    #region TimeoutError Tests

    [Fact]
    public void TimeoutError_WithValidParameters_ReturnsFormattedMessage()
    {
        // Arrange
        string operation = "database query";
        int timeoutSeconds = 30;

        // Act
        string result = ErrorMessageBuilder.TimeoutError(operation, timeoutSeconds);

        // Assert
        result.Should().Contain("The operation 'database query' timed out after 30 seconds");
        result.Should().Contain("Suggestions:");
        result.Should().Contain("Try the operation again");
        result.Should().Contain("Check your network connection");
        result.Should().Contain("Verify the target service is responsive");
        result.Should().Contain("Consider increasing the timeout configuration if this issue persists");
    }

    [Fact]
    public void TimeoutError_WithZeroTimeout_HandlesCorrectly()
    {
        // Arrange
        string operation = "immediate operation";
        int timeoutSeconds = 0;

        // Act
        string result = ErrorMessageBuilder.TimeoutError(operation, timeoutSeconds);

        // Assert
        result.Should().Contain("timed out after 0 seconds");
        result.Should().Contain("Try the operation again");
    }

    [Fact]
    public void TimeoutError_WithLargeTimeout_HandlesCorrectly()
    {
        // Arrange
        string operation = "long running process";
        int timeoutSeconds = 3600;

        // Act
        string result = ErrorMessageBuilder.TimeoutError(operation, timeoutSeconds);

        // Assert
        result.Should().Contain("timed out after 3600 seconds");
        result.Should().Contain("Consider increasing the timeout configuration if this issue persists");
    }

    #endregion

    #region DependencyError Tests

    [Fact]
    public void DependencyError_WithValidParameters_ReturnsFormattedMessage()
    {
        // Arrange
        string missingService = "IEmailService";
        string requiredFor = "sending notification emails";

        // Act
        string result = ErrorMessageBuilder.DependencyError(missingService, requiredFor);

        // Assert
        result.Should().Contain("Required service 'IEmailService' is not registered and is needed for sending notification emails");
        result.Should().Contain("To resolve this issue:");
        result.Should().Contain("Register the IEmailService service in your DI container");
        result.Should().Contain("Check your service registration configuration");
        result.Should().Contain("Verify all required NuGet packages are installed");
        result.Should().Contain("Review the CRISP setup documentation for required dependencies");
    }

    [Fact]
    public void DependencyError_ServiceNameInSolution_IncludesServiceName()
    {
        // Arrange
        string missingService = "ICacheService";
        string requiredFor = "caching user sessions";

        // Act
        string result = ErrorMessageBuilder.DependencyError(missingService, requiredFor);

        // Assert
        result.Should().Contain("Register the ICacheService service in your DI container");
    }

    #endregion

    #region RateLimitError Tests

    [Fact]
    public void RateLimitError_WithValidParameters_ReturnsFormattedMessage()
    {
        // Arrange
        string operation = "API requests";
        int retryAfterSeconds = 60;

        // Act
        string result = ErrorMessageBuilder.RateLimitError(operation, retryAfterSeconds);

        // Assert
        result.Should().Contain("Rate limit exceeded for 'API requests'. Too many requests have been made");
        result.Should().Contain("Suggestions:");
        result.Should().Contain("Wait 60 seconds before trying again");
        result.Should().Contain("Reduce the frequency of your requests");
        result.Should().Contain("Consider implementing request batching if applicable");
        result.Should().Contain("Contact support if you need higher rate limits");
    }

    [Fact]
    public void RateLimitError_WithZeroRetryAfter_HandlesCorrectly()
    {
        // Arrange
        string operation = "login attempts";
        int retryAfterSeconds = 0;

        // Act
        string result = ErrorMessageBuilder.RateLimitError(operation, retryAfterSeconds);

        // Assert
        result.Should().Contain("Wait 0 seconds before trying again");
        result.Should().Contain("Reduce the frequency of your requests");
    }

    [Fact]
    public void RateLimitError_WithLargeRetryAfter_HandlesCorrectly()
    {
        // Arrange
        string operation = "file uploads";
        int retryAfterSeconds = 3600;

        // Act
        string result = ErrorMessageBuilder.RateLimitError(operation, retryAfterSeconds);

        // Assert
        result.Should().Contain("Wait 3600 seconds before trying again");
        result.Should().Contain("Contact support if you need higher rate limits");
    }

    #endregion

    #region Message Format Tests

    [Fact]
    public void AllMethods_BulletPointFormat_UseBulletPoints()
    {
        // Act
        var notFoundResult = ErrorMessageBuilder.NotFound("User", 1);
        var unauthorizedResult = ErrorMessageBuilder.Unauthorized("access", "resource");
        var configResult = ErrorMessageBuilder.ConfigurationError("component", "issue", "solution");
        var timeoutResult = ErrorMessageBuilder.TimeoutError("operation", 30);
        var dependencyResult = ErrorMessageBuilder.DependencyError("service", "purpose");
        var rateLimitResult = ErrorMessageBuilder.RateLimitError("operation", 60);

        // Assert
        notFoundResult.Should().Contain("• ");
        unauthorizedResult.Should().Contain("• ");
        configResult.Should().Contain("• ");
        timeoutResult.Should().Contain("• ");
        dependencyResult.Should().Contain("• ");
        rateLimitResult.Should().Contain("• ");
    }

    [Fact]
    public void AllMethods_MessageStructure_ContainMainMessageAndSuggestions()
    {
        // Act
        var notFoundResult = ErrorMessageBuilder.NotFound("User", 1);
        var unauthorizedResult = ErrorMessageBuilder.Unauthorized("access", "resource");
        var configResult = ErrorMessageBuilder.ConfigurationError("component", "issue", "solution");
        var timeoutResult = ErrorMessageBuilder.TimeoutError("operation", 30);
        var dependencyResult = ErrorMessageBuilder.DependencyError("service", "purpose");
        var rateLimitResult = ErrorMessageBuilder.RateLimitError("operation", 60);

        // Assert - All should have structured format with suggestions
        var results = new[] { notFoundResult, unauthorizedResult, configResult, timeoutResult, dependencyResult, rateLimitResult };
        
        foreach (var result in results)
        {
            result.Should().Contain("\n\n");
            result.Should().MatchRegex(@".*\n\n(Suggestions|To resolve this issue):.*");
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void NotFound_WithComplexIdentifier_HandlesCorrectly()
    {
        // Arrange
        var complexIdentifier = new { Id = 123, Type = "Order" };

        // Act
        string result = ErrorMessageBuilder.NotFound("Entity", complexIdentifier);

        // Assert
        result.Should().Contain(complexIdentifier.ToString());
    }

    [Fact]
    public void ValidationError_WithComplexValue_HandlesCorrectly()
    {
        // Arrange
        var complexValue = new List<string> { "item1", "item2" };

        // Act
        string result = ErrorMessageBuilder.ValidationError("List", complexValue, "Must have at least 3 items");

        // Assert
        result.Should().Contain(complexValue.ToString());
        result.Should().Contain("Must have at least 3 items");
    }

    #endregion
}