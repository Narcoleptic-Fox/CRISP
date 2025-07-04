using Crisp.Exceptions;

namespace Crisp.Core.Tests.Exceptions;

public class NotFoundExceptionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesExceptionWithDefaultMessage()
    {
        // Act
        NotFoundException exception = new();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("The requested resource was not found. Please verify the resource identifier and try again.");
        exception.ResourceType.Should().Be(string.Empty);
        exception.ResourceId.Should().Be(string.Empty);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessage_CreatesExceptionWithSpecifiedMessage()
    {
        // Arrange
        const string expectedMessage = "Custom not found message";

        // Act
        NotFoundException exception = new(expectedMessage);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.ResourceType.Should().Be(string.Empty);
        exception.ResourceId.Should().Be(string.Empty);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithIdAndEntityName_CreatesExceptionWithFormattedMessage()
    {
        // Arrange
        object id = 123;
        const string entityName = "User";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.Message.Should().Be("The User with identifier '123' was not found. Please verify the user ID exists and try again.");
        exception.ResourceType.Should().Be("User");
        exception.ResourceId.Should().Be("123");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithStringId_HandlesStringIdentifier()
    {
        // Arrange
        const string id = "USER-ABC-123";
        const string entityName = "Customer";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.Message.Should().Be("The Customer with identifier 'USER-ABC-123' was not found. Please verify the customer ID exists and try again.");
        exception.ResourceType.Should().Be("Customer");
        exception.ResourceId.Should().Be("USER-ABC-123");
    }

    [Fact]
    public void Constructor_WithGuidId_HandlesGuidIdentifier()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        const string entityName = "Order";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.Message.Should().Contain($"The Order with identifier '{id}' was not found");
        exception.ResourceType.Should().Be("Order");
        exception.ResourceId.Should().Be(id.ToString());
    }

    [Fact]
    public void Constructor_WithNullId_HandlesNullIdentifier()
    {
        // Arrange
        object? id = null;
        const string entityName = "Product";

        // Act
        NotFoundException exception = new(id!, entityName);

        // Assert
        exception.Message.Should().Contain("The Product with identifier '' was not found");
        exception.ResourceType.Should().Be("Product");
        exception.ResourceId.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_WithComplexId_HandlesToStringMethod()
    {
        // Arrange
        var id = new { UserId = 123, TenantId = "ABC" };
        const string entityName = "UserProfile";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.Message.Should().Contain("UserProfile");
        exception.Message.Should().Contain(id.ToString());
        exception.ResourceType.Should().Be("UserProfile");
        exception.ResourceId.Should().Be(id.ToString());
    }

    [Fact]
    public void Constructor_WithIdEntityNameAndSuggestions_IncludesSuggestions()
    {
        // Arrange
        object id = 456;
        const string entityName = "Document";
        const string suggestions = "Make sure the document hasn't been archived.";

        // Act
        NotFoundException exception = new(id, entityName, suggestions);

        // Assert
        exception.Message.Should().Be("The Document with identifier '456' was not found. Make sure the document hasn't been archived.");
        exception.ResourceType.Should().Be("Document");
        exception.ResourceId.Should().Be("456");
    }

    [Fact]
    public void Constructor_WithEmptySuggestions_OnlyIncludesMainMessage()
    {
        // Arrange
        object id = 789;
        const string entityName = "Invoice";
        const string suggestions = "";

        // Act
        NotFoundException exception = new(id, entityName, suggestions);

        // Assert
        exception.Message.Should().Be("The Invoice with identifier '789' was not found. ");
        exception.ResourceType.Should().Be("Invoice");
        exception.ResourceId.Should().Be("789");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void ResourceType_CanBeSetAndRetrieved()
    {
        // Arrange
        NotFoundException exception = new();
        const string expectedResourceType = "CustomResource";

        // Act
        exception.ResourceType = expectedResourceType;

        // Assert
        exception.ResourceType.Should().Be(expectedResourceType);
    }

    [Fact]
    public void ResourceId_CanBeSetAndRetrieved()
    {
        // Arrange
        NotFoundException exception = new();
        const string expectedResourceId = "CUSTOM-ID-123";

        // Act
        exception.ResourceId = expectedResourceId;

        // Assert
        exception.ResourceId.Should().Be(expectedResourceId);
    }

    [Fact]
    public void Properties_DefaultToEmptyString()
    {
        // Act
        NotFoundException exception = new("Custom message");

        // Assert
        exception.ResourceType.Should().Be(string.Empty);
        exception.ResourceId.Should().Be(string.Empty);
    }

    [Fact]
    public void Properties_CanBeSetToNull()
    {
        // Arrange
        NotFoundException exception = new(123, "User")
        {
            // Act
            ResourceType = null!,
            ResourceId = null!
        };

        // Assert
        exception.ResourceType.Should().BeNull();
        exception.ResourceId.Should().BeNull();
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void NotFoundException_InheritsFromCrispException()
    {
        // Act
        NotFoundException exception = new();

        // Assert
        exception.Should().BeAssignableTo<CrispException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void NotFoundException_CanBeCaughtAsCrispException()
    {
        // Arrange
        CrispException? caughtException = null;

        // Act
        try
        {
            throw new NotFoundException(123, "User");
        }
        catch (CrispException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<NotFoundException>();
    }

    #endregion

    #region Message Formatting Tests

    [Fact]
    public void Constructor_WithEntityNameCasing_PreservesOriginalCasing()
    {
        // Arrange
        object id = 1;
        const string entityName = "UserAccount";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.Message.Should().Contain("The UserAccount with identifier");
        exception.Message.Should().Contain("verify the useraccount ID exists");
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInEntityName_HandlesCorrectly()
    {
        // Arrange
        object id = 1;
        const string entityName = "User-Profile";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.Message.Should().Contain("The User-Profile with identifier");
        exception.Message.Should().Contain("verify the user-profile ID exists");
    }

    [Fact]
    public void Constructor_WithUnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        object id = "测试-123";
        const string entityName = "用户";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.Message.Should().Contain("The 用户 with identifier '测试-123'");
        exception.ResourceType.Should().Be("用户");
        exception.ResourceId.Should().Be("测试-123");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithVeryLongId_HandlesCorrectly()
    {
        // Arrange
        string longId = new('A', 1000);
        const string entityName = "LargeEntity";

        // Act
        NotFoundException exception = new(longId, entityName);

        // Assert
        exception.ResourceId.Should().Be(longId);
        exception.Message.Should().Contain(longId);
    }

    [Fact]
    public void Constructor_WithVeryLongEntityName_HandlesCorrectly()
    {
        // Arrange
        object id = 1;
        string longEntityName = new('B', 500);

        // Act
        NotFoundException exception = new(id, longEntityName);

        // Assert
        exception.ResourceType.Should().Be(longEntityName);
        exception.Message.Should().Contain(longEntityName);
    }

    [Fact]
    public void Constructor_WithZeroId_HandlesCorrectly()
    {
        // Arrange
        object id = 0;
        const string entityName = "ZeroIdEntity";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.ResourceId.Should().Be("0");
        exception.Message.Should().Contain("'0'");
    }

    [Fact]
    public void Constructor_WithNegativeId_HandlesCorrectly()
    {
        // Arrange
        object id = -123;
        const string entityName = "NegativeIdEntity";

        // Act
        NotFoundException exception = new(id, entityName);

        // Assert
        exception.ResourceId.Should().Be("-123");
        exception.Message.Should().Contain("'-123'");
    }

    #endregion

    #region Data Property Tests

    [Fact]
    public void Exception_CanStoreAdditionalContextData()
    {
        // Arrange
        NotFoundException exception = new(123, "User");

        // Act
        exception.Data["SearchCriteria"] = "email=test@example.com";
        exception.Data["AttemptedOperation"] = "GetUserById";
        exception.Data["Timestamp"] = DateTime.UtcNow;

        // Assert
        exception.Data["SearchCriteria"].Should().Be("email=test@example.com");
        exception.Data["AttemptedOperation"].Should().Be("GetUserById");
        exception.Data["Timestamp"].Should().BeOfType<DateTime>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Exception_UsedInApplicationScenario_WorksCorrectly()
    {
        // Arrange
        const int userId = 999;
        const string userEntityName = "User";
        NotFoundException? caughtException = null;

        // Act - Simulate a service method that throws NotFoundException
        try
        {
            SimulateUserNotFound(userId, userEntityName);
        }
        catch (NotFoundException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException!.ResourceType.Should().Be(userEntityName);
        caughtException.ResourceId.Should().Be(userId.ToString());
        caughtException.Message.Should().Contain("User");
        caughtException.Message.Should().Contain("999");
    }

    private static void SimulateUserNotFound(int userId, string entityName) =>
        // Simulate checking a repository and not finding the user
        throw new NotFoundException(userId, entityName);

    [Fact]
    public void Exception_WithCascadingNotFound_PreservesOriginalContext()
    {
        // Arrange
        const int orderId = 456;
        const int customerId = 789;
        Exception? caughtException = null;

        // Act
        try
        {
            SimulateCascadingNotFound(orderId, customerId);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<NotFoundException>();
        caughtException!.InnerException.Should().NotBeNull();
        caughtException.InnerException.Should().BeOfType<NotFoundException>();

        NotFoundException outerException = (NotFoundException)caughtException;
        NotFoundException innerException = (NotFoundException)caughtException.InnerException!;

        outerException.ResourceType.Should().Be("Order");
        innerException.ResourceType.Should().Be("Customer");
    }

    private static void SimulateCascadingNotFound(int orderId, int customerId)
    {
        try
        {
            throw new NotFoundException(customerId, "Customer");
        }
        catch (NotFoundException innerEx)
        {
            throw new NotFoundException(orderId, "Order", "Order not found because the associated customer was not found.", innerEx);
        }
    }

    #endregion
}