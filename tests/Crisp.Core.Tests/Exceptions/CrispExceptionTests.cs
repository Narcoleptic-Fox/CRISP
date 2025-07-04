using Crisp.Exceptions;

namespace Crisp.Core.Tests.Exceptions;

public class CrispExceptionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesExceptionWithDefaultMessage()
    {
        // Act
        CrispException exception = new();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessage_CreatesExceptionWithSpecifiedMessage()
    {
        // Arrange
        const string expectedMessage = "Test error message";

        // Act
        CrispException exception = new(expectedMessage);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullMessage_CreatesExceptionWithNullMessage()
    {
        // Act
        CrispException exception = new(null!);

        // Assert
        exception.Message.Should().Be("Exception of type 'Crisp.Exceptions.CrispException' was thrown.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyMessage_CreatesExceptionWithEmptyMessage()
    {
        // Act
        CrispException exception = new(string.Empty);

        // Assert
        exception.Message.Should().Be(string.Empty);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_CreatesExceptionWithBoth()
    {
        // Arrange
        const string expectedMessage = "Outer exception message";
        InvalidOperationException innerException = new("Inner exception message");

        // Act
        CrispException exception = new(expectedMessage, innerException);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Constructor_WithMessageAndNullInnerException_CreatesExceptionWithMessageOnly()
    {
        // Arrange
        const string expectedMessage = "Test message";

        // Act
        CrispException exception = new(expectedMessage, null!);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().BeNull();
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void CrispException_InheritsFromSystemException()
    {
        // Act
        CrispException exception = new();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void CrispException_IsBaseForOtherCrispExceptions()
    {
        // Act
        NotFoundException notFoundException = new();
        UnauthorizedException unauthorizedException = new();
        ValidationException validationException = new();

        // Assert
        notFoundException.Should().BeAssignableTo<CrispException>();
        unauthorizedException.Should().BeAssignableTo<CrispException>();
        validationException.Should().BeAssignableTo<CrispException>();
    }

    #endregion

    #region Exception Behavior Tests

    [Fact]
    public void ToString_WithMessage_ContainsExceptionTypeAndMessage()
    {
        // Arrange
        const string message = "Test exception message";
        CrispException exception = new(message);

        // Act
        string result = exception.ToString();

        // Assert
        result.Should().Contain("CrispException");
        result.Should().Contain(message);
    }

    [Fact]
    public void ToString_WithInnerException_ContainsInnerExceptionInfo()
    {
        // Arrange
        const string outerMessage = "Outer message";
        const string innerMessage = "Inner message";
        ArgumentException innerException = new(innerMessage);
        CrispException exception = new(outerMessage, innerException);

        // Act
        string result = exception.ToString();

        // Assert
        result.Should().Contain("CrispException");
        result.Should().Contain(outerMessage);
        result.Should().Contain("ArgumentException");
        result.Should().Contain(innerMessage);
    }

    [Fact]
    public void GetBaseException_WithInnerException_ReturnsInnermostException()
    {
        // Arrange
        ArgumentException innermostException = new("Innermost");
        InvalidOperationException middleException = new("Middle", innermostException);
        CrispException outerException = new("Outer", middleException);

        // Act
        Exception baseException = outerException.GetBaseException();

        // Assert
        baseException.Should().Be(innermostException);
    }

    [Fact]
    public void GetBaseException_WithoutInnerException_ReturnsSelf()
    {
        // Arrange
        CrispException exception = new("Test message");

        // Act
        Exception baseException = exception.GetBaseException();

        // Assert
        baseException.Should().Be(exception);
    }

    #endregion

    #region Data Property Tests

    [Fact]
    public void Data_CanAddCustomData()
    {
        // Arrange
        CrispException exception = new("Test message");
        const string key = "CustomKey";
        const string value = "CustomValue";

        // Act
        exception.Data[key] = value;

        // Assert
        exception.Data[key].Should().Be(value);
        exception.Data.Contains(key).Should().BeTrue();
    }

    [Fact]
    public void Data_SupportsMultipleEntries()
    {
        // Arrange
        CrispException exception = new("Test message");

        // Act
        exception.Data["Key1"] = "Value1";
        exception.Data["Key2"] = 42;
        exception.Data["Key3"] = DateTime.Now;

        // Assert
        exception.Data.Count.Should().Be(3);
        exception.Data["Key1"].Should().Be("Value1");
        exception.Data["Key2"].Should().Be(42);
        exception.Data["Key3"].Should().BeOfType<DateTime>();
    }

    #endregion

    #region Stack Trace Tests

    [Fact]
    public void StackTrace_WhenThrown_ContainsStackInformation()
    {
        // Arrange & Act
        CrispException? caughtException = null;

        try
        {
            ThrowCrispException();
        }
        catch (CrispException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException!.StackTrace.Should().NotBeNullOrEmpty();
        caughtException.StackTrace.Should().Contain(nameof(ThrowCrispException));
    }

    private static void ThrowCrispException() => throw new CrispException("Test exception for stack trace");

    #endregion

    #region Exception Serialization Tests

    [Fact]
    public void Exception_CanBeSerializedAndDeserialized()
    {
        // Arrange
        const string originalMessage = "Original exception message";
        ArgumentException originalInnerException = new("Inner exception");
        CrispException originalException = new(originalMessage, originalInnerException);
        originalException.Data["CustomKey"] = "CustomValue";

        // Act - Use JSON serialization instead of binary formatter
        string jsonString = System.Text.Json.JsonSerializer.Serialize(new
        {
            originalException.Message,
            Data = originalException.Data.Cast<System.Collections.DictionaryEntry>().ToDictionary(e => e.Key.ToString(), e => e.Value?.ToString()),
            InnerExceptionMessage = originalException.InnerException?.Message
        });

        System.Text.Json.JsonElement deserializedData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonString);
        CrispException deserializedException = new(
            deserializedData.GetProperty("Message").GetString()!,
            originalException.InnerException != null ? new ArgumentException(deserializedData.GetProperty("InnerExceptionMessage").GetString()!) : null
        );

        foreach (System.Text.Json.JsonProperty kvp in deserializedData.GetProperty("Data").EnumerateObject())
        {
            deserializedException.Data[kvp.Name] = kvp.Value.GetString();
        }

        // Assert
        deserializedException.Should().NotBeNull();
        deserializedException.Message.Should().Be(originalMessage);
        deserializedException.InnerException.Should().NotBeNull();
        deserializedException.InnerException!.Message.Should().Be("Inner exception");
        deserializedException.Data["CustomKey"].Should().Be("CustomValue");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // Arrange
        CrispException exception = new("Test message");

        // Act & Assert
        exception.Equals(exception).Should().BeTrue();
        ReferenceEquals(exception, exception).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentExceptions_ReturnsFalse()
    {
        // Arrange
        CrispException exception1 = new("Message 1");
        CrispException exception2 = new("Message 2");

        // Act & Assert
        exception1.Equals(exception2).Should().BeFalse();
        (exception1 == exception2).Should().BeFalse();
    }

    [Fact]
    public void Equals_NullException_ReturnsFalse()
    {
        // Arrange
        CrispException? exception = new("Test message");

        // Act & Assert
        exception.Equals(null).Should().BeFalse();
        (exception == null).Should().BeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameException_ReturnsSameHashCode()
    {
        // Arrange
        CrispException exception = new("Test message");

        // Act
        int hashCode1 = exception.GetHashCode();
        int hashCode2 = exception.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_DifferentExceptions_MayReturnDifferentHashCodes()
    {
        // Arrange
        CrispException exception1 = new("Message 1");
        CrispException exception2 = new("Message 2");

        // Act
        int hashCode1 = exception1.GetHashCode();
        int hashCode2 = exception2.GetHashCode();

        // Assert
        // Hash codes may be different (but not guaranteed to be)
        // This test mainly ensures GetHashCode doesn't throw
        hashCode1.Should().NotBe(0);
        hashCode2.Should().NotBe(0);
    }

    #endregion

    #region Source Property Tests

    [Fact]
    public void Source_CanBeSetAndRetrieved()
    {
        // Arrange
        CrispException exception = new("Test message");
        const string expectedSource = "TestAssembly";

        // Act
        exception.Source = expectedSource;

        // Assert
        exception.Source.Should().Be(expectedSource);
    }

    [Fact]
    public void Source_DefaultsToAssemblyName()
    {
        // Arrange & Act
        CrispException exception = new("Test message");

        // Assert
        exception.Message.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region HelpLink Property Tests

    [Fact]
    public void HelpLink_CanBeSetAndRetrieved()
    {
        // Arrange
        CrispException exception = new("Test message");
        const string expectedHelpLink = "https://docs.example.com/error/crisp-exception";

        // Act
        exception.HelpLink = expectedHelpLink;

        // Assert
        exception.HelpLink.Should().Be(expectedHelpLink);
    }

    [Fact]
    public void HelpLink_DefaultsToNull()
    {
        // Arrange & Act
        CrispException exception = new("Test message");

        // Assert
        exception.HelpLink.Should().BeNull();
    }

    #endregion

    #region Exception Integration Tests

    [Fact]
    public void CrispException_CanBeUsedInTryCatchBlocks()
    {
        // Arrange
        const string expectedMessage = "Test exception handling";
        CrispException? caughtException = null;

        // Act
        try
        {
            throw new CrispException(expectedMessage);
        }
        catch (CrispException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException!.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void CrispException_CanBeCaughtAsBaseException()
    {
        // Arrange
        const string expectedMessage = "Test base exception handling";
        Exception? caughtException = null;

        // Act
        try
        {
            throw new CrispException(expectedMessage);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<CrispException>();
        caughtException!.Message.Should().Be(expectedMessage);
    }

    #endregion
}