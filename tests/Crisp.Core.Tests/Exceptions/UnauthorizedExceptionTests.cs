using Crisp.Exceptions;

namespace Crisp.Core.Tests.Exceptions;

public class UnauthorizedExceptionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesExceptionWithDefaultMessage()
    {
        // Act
        var exception = new UnauthorizedException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Access denied. You do not have sufficient permissions to perform this operation. Please contact your administrator or verify your authentication status.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessage_CreatesExceptionWithSpecifiedMessage()
    {
        // Arrange
        const string expectedMessage = "Custom unauthorized message";

        // Act
        var exception = new UnauthorizedException(expectedMessage);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_CreatesExceptionWithBoth()
    {
        // Arrange
        const string expectedMessage = "Outer unauthorized message";
        var innerException = new InvalidOperationException("Inner exception message");

        // Act
        var exception = new UnauthorizedException(expectedMessage, innerException);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Constructor_WithOperationAndResource_CreatesFormattedMessage()
    {
        // Arrange
        const string operation = "delete";
        const string resourceName = "user accounts";

        // Act
        var exception = new UnauthorizedException(operation, resourceName);

        // Assert
        exception.Message.Should().Be("You are not authorized to delete the user accounts. Please ensure you have the required permissions or contact your administrator for access.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithOperationResourceAndPermission_CreatesDetailedMessage()
    {
        // Arrange
        const string operation = "read";
        const string resourceName = "financial reports";
        const string requiredPermission = "Finance.Read";

        // Act
        var exception = new UnauthorizedException(operation, resourceName, requiredPermission);

        // Assert
        exception.Message.Should().Be("You are not authorized to read the financial reports. This operation requires 'Finance.Read' permission. Please contact your administrator to request access.");
        exception.InnerException.Should().BeNull();
    }

    #endregion

    #region Message Formatting Tests

    [Fact]
    public void Constructor_WithVariousOperations_FormatsCorrectly()
    {
        // Arrange & Act
        var createException = new UnauthorizedException("create", "projects");
        var updateException = new UnauthorizedException("update", "settings");
        var deleteException = new UnauthorizedException("delete", "files");
        var accessException = new UnauthorizedException("access", "admin panel");

        // Assert
        createException.Message.Should().Contain("You are not authorized to create the projects");
        updateException.Message.Should().Contain("You are not authorized to update the settings");
        deleteException.Message.Should().Contain("You are not authorized to delete the files");
        accessException.Message.Should().Contain("You are not authorized to access the admin panel");
    }

    [Fact]
    public void Constructor_WithDifferentPermissions_IncludesSpecificPermission()
    {
        // Arrange & Act
        var adminException = new UnauthorizedException("access", "admin features", "Admin");
        var managerException = new UnauthorizedException("approve", "purchases", "Manager.Approval");
        var viewException = new UnauthorizedException("view", "reports", "Reports.View");

        // Assert
        adminException.Message.Should().Contain("This operation requires 'Admin' permission");
        managerException.Message.Should().Contain("This operation requires 'Manager.Approval' permission");
        viewException.Message.Should().Contain("This operation requires 'Reports.View' permission");
    }

    [Fact]
    public void Constructor_WithEmptyStrings_HandlesGracefully()
    {
        // Act
        var emptyOperationException = new UnauthorizedException("", "resource");
        var emptyResourceException = new UnauthorizedException("operation", "");
        var emptyPermissionException = new UnauthorizedException("operation", "resource", "");

        // Assert
        emptyOperationException.Message.Should().Contain("You are not authorized to  the resource");
        emptyResourceException.Message.Should().Contain("You are not authorized to operation the ");
        emptyPermissionException.Message.Should().Contain("This operation requires '' permission");
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        const string operation = "read/write";
        const string resourceName = "user-profiles & settings";
        const string permission = "User.Read+Write";

        // Act
        var exception = new UnauthorizedException(operation, resourceName, permission);

        // Assert
        exception.Message.Should().Contain("read/write");
        exception.Message.Should().Contain("user-profiles & settings");
        exception.Message.Should().Contain("User.Read+Write");
    }

    [Fact]
    public void Constructor_WithUnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        const string operation = "访问";
        const string resourceName = "用户数据";
        const string permission = "数据.读取";

        // Act
        var exception = new UnauthorizedException(operation, resourceName, permission);

        // Assert
        exception.Message.Should().Contain("访问");
        exception.Message.Should().Contain("用户数据");
        exception.Message.Should().Contain("数据.读取");
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void UnauthorizedException_InheritsFromCrispException()
    {
        // Act
        var exception = new UnauthorizedException();

        // Assert
        exception.Should().BeAssignableTo<CrispException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void UnauthorizedException_CanBeCaughtAsCrispException()
    {
        // Arrange
        CrispException? caughtException = null;

        // Act
        try
        {
            throw new UnauthorizedException("access", "resource");
        }
        catch (CrispException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<UnauthorizedException>();
    }

    [Fact]
    public void UnauthorizedException_CanBeCaughtAsBaseException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            throw new UnauthorizedException("read", "documents", "Document.Read");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<UnauthorizedException>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithNullStrings_HandlesGracefully()
    {
        // Act & Assert - These should not throw exceptions
        FluentActions.Invoking(() => new UnauthorizedException((string)null!))
            .Should().NotThrow();

        FluentActions.Invoking(() => new UnauthorizedException("operation", (string)null!))
            .Should().NotThrow();

        FluentActions.Invoking(() => new UnauthorizedException("operation", "resource", null!))
            .Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithVeryLongStrings_HandlesCorrectly()
    {
        // Arrange
        string longOperation = new string('A', 1000);
        string longResource = new string('B', 1000);
        string longPermission = new string('C', 1000);

        // Act
        var exception = new UnauthorizedException(longOperation, longResource, longPermission);

        // Assert
        exception.Message.Should().Contain(longOperation);
        exception.Message.Should().Contain(longResource);
        exception.Message.Should().Contain(longPermission);
    }

    [Fact]
    public void Constructor_WithWhitespaceStrings_HandlesCorrectly()
    {
        // Act
        var spacesException = new UnauthorizedException("   ", "   ", "   ");
        var tabsException = new UnauthorizedException("\t\t", "\t\t", "\t\t");
        var newlinesException = new UnauthorizedException("\n\n", "\n\n", "\n\n");

        // Assert
        spacesException.Message.Should().NotBeEmpty();
        tabsException.Message.Should().NotBeEmpty();
        newlinesException.Message.Should().NotBeEmpty();
    }

    #endregion

    #region Data Property Tests

    [Fact]
    public void Exception_CanStoreAdditionalSecurityContext()
    {
        // Arrange
        var exception = new UnauthorizedException("read", "sensitive data", "Security.Read");

        // Act
        exception.Data["UserId"] = "user123";
        exception.Data["AttemptedResource"] = "/api/sensitive";
        exception.Data["UserRoles"] = new[] { "User", "Guest" };
        exception.Data["Timestamp"] = DateTime.UtcNow;
        exception.Data["IPAddress"] = "192.168.1.1";

        // Assert
        exception.Data["UserId"].Should().Be("user123");
        exception.Data["AttemptedResource"].Should().Be("/api/sensitive");
        exception.Data["UserRoles"].Should().BeEquivalentTo(new[] { "User", "Guest" });
        exception.Data["Timestamp"].Should().BeOfType<DateTime>();
        exception.Data["IPAddress"].Should().Be("192.168.1.1");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Exception_UsedInAuthorizationScenario_WorksCorrectly()
    {
        // Arrange
        const string operation = "delete";
        const string resource = "user data";
        const string requiredPermission = "Admin.Delete";
        UnauthorizedException? caughtException = null;

        // Act - Simulate an authorization check that fails
        try
        {
            SimulateAuthorizationFailure(operation, resource, requiredPermission);
        }
        catch (UnauthorizedException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException!.Message.Should().Contain(operation);
        caughtException.Message.Should().Contain(resource);
        caughtException.Message.Should().Contain(requiredPermission);
    }

    private static void SimulateAuthorizationFailure(string operation, string resource, string requiredPermission)
    {
        // Simulate checking user permissions
        bool hasPermission = false; // User doesn't have permission
        
        if (!hasPermission)
        {
            throw new UnauthorizedException(operation, resource, requiredPermission);
        }
    }

    [Fact]
    public void Exception_WithNestedAuthorizationFailure_PreservesContext()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            SimulateNestedAuthorizationFailure();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<UnauthorizedException>();
        caughtException!.InnerException.Should().NotBeNull();
        caughtException.InnerException.Should().BeOfType<UnauthorizedException>();

        var outerException = (UnauthorizedException)caughtException;
        var innerException = (UnauthorizedException)caughtException.InnerException!;

        outerException.Message.Should().Contain("cascade delete");
        innerException.Message.Should().Contain("delete");
    }

    private static void SimulateNestedAuthorizationFailure()
    {
        try
        {
            throw new UnauthorizedException("delete", "child records", "Admin.Delete");
        }
        catch (UnauthorizedException innerEx)
        {
            throw new UnauthorizedException("cascade delete operation failed", innerEx);
        }
    }

    [Fact]
    public void Exception_InAPIScenario_ProvidesAppropriateDetails()
    {
        // Arrange
        var exception = new UnauthorizedException("access", "API endpoint", "API.Read");
        
        // Act - Add typical API context
        exception.Data["Endpoint"] = "/api/v1/users";
        exception.Data["Method"] = "GET";
        exception.Data["UserAgent"] = "Mozilla/5.0";
        exception.Data["RequestId"] = Guid.NewGuid();

        // Assert
        exception.Message.Should().Contain("access");
        exception.Message.Should().Contain("API endpoint");
        exception.Message.Should().Contain("API.Read");
        exception.Data.Count.Should().Be(4);
    }

    #endregion

    #region Stack Trace Tests

    [Fact]
    public void StackTrace_WhenThrown_ContainsStackInformation()
    {
        // Arrange & Act
        UnauthorizedException? caughtException = null;
        
        try
        {
            ThrowUnauthorizedException();
        }
        catch (UnauthorizedException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException!.StackTrace.Should().NotBeNullOrEmpty();
        caughtException.StackTrace.Should().Contain(nameof(ThrowUnauthorizedException));
    }

    private static void ThrowUnauthorizedException()
    {
        throw new UnauthorizedException("test", "resource", "permission");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WithBasicException_ContainsExpectedInformation()
    {
        // Arrange
        var exception = new UnauthorizedException("read", "files");

        // Act
        string result = exception.ToString();

        // Assert
        result.Should().Contain("UnauthorizedException");
        result.Should().Contain("You are not authorized to read the files");
    }

    [Fact]
    public void ToString_WithInnerException_ContainsInnerExceptionInfo()
    {
        // Arrange
        var innerException = new InvalidOperationException("Authentication failed");
        var exception = new UnauthorizedException("access failed", innerException);

        // Act
        string result = exception.ToString();

        // Assert
        result.Should().Contain("UnauthorizedException");
        result.Should().Contain("InvalidOperationException");
        result.Should().Contain("Authentication failed");
        result.Should().Contain("access failed");
    }

    #endregion
}