using CRISP.Core.Behaviors;
using CRISP.Core.Interfaces;
using CRISP.Core.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ValidationBehavior<TestRequest, string>>> _loggerMock;
    private readonly ValidationOptions _options;

    public ValidationBehaviorTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<ValidationBehavior<TestRequest, string>>>();
        _options = new ValidationOptions
        {
            ValidateChildObjects = false,
            MaxChildValidationDepth = 3,
            LogValidationFailures = true,
            ThrowExceptionOnValidationFailure = true,
            SkipValidationIfNoValidatorsRegistered = true
        };
    }

    [Fact]
    public async Task Handle_WithNoValidators_ProceedsToNextDelegate()
    {
        // Arrange
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IValidator<TestRequest>>)))
            .Returns(Enumerable.Empty<IValidator<TestRequest>>());

        ValidationBehavior<TestRequest, string> behavior = new(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _options);

        TestRequest request = new() { Name = "Test" };
        string expectedResult = "Success";

        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>(expectedResult);

        // Act
        string result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Handle_WithPassingValidation_ProceedsToNextDelegate()
    {
        // Arrange
        Mock<IValidator<TestRequest>> validator = new();
        validator.Setup(v => v.Validate(It.IsAny<TestRequest>()))
            .Returns(new ValidationResult());

        _serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>()))
            .Returns(new[] { validator.Object });

        ValidationBehavior<TestRequest, string> behavior = new(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _options);

        TestRequest request = new() { Name = "Test" };
        string expectedResult = "Success";

        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>(expectedResult);

        // Act
        string result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        validator.Verify(v => v.Validate(request), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFailingValidation_ThrowsValidationException()
    {
        // Arrange
        IEnumerable<ValidationError> errors =
        [
            new ValidationError("Name", "Name is required"),
            new ValidationError("Age", "Age must be positive")
        ];

        ValidationResult validationResult = new()
        {
            IsValid = false,
            Errors = errors
        };

        Mock<IValidator<TestRequest>> validator = new();
        validator.Setup(v => v.Validate(It.IsAny<TestRequest>()))
            .Returns(validationResult);

        IEnumerable<IValidator<TestRequest>> serviceCollection = [validator.Object];

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IValidator<TestRequest>>)))
            .Returns(serviceCollection);

        ValidationBehavior<TestRequest, string> behavior = new(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _options);

        TestRequest request = new();
        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>("Should not reach this");

        // Act
        Func<Task> act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Message.Contains("Name is required") && ex.Message.Contains("Age must be positive"));
    }

    [Fact]
    public async Task Handle_NoValidatorsAndSkipValidationDisabled_ThrowsException()
    {
        // Arrange
        _serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>()))
            .Returns(Enumerable.Empty<IValidator<TestRequest>>());

        ValidationOptions options = new()
        {
            SkipValidationIfNoValidatorsRegistered = false
        };

        ValidationBehavior<TestRequest, string> behavior = new(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            options);

        TestRequest request = new();
        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>("Should not reach this");

        // Act
        Func<Task> act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"No validators registered for {typeof(TestRequest).Name}");
    }

    [Fact]
    public async Task Handle_ValidationFailsButThrowExceptionDisabled_LogsWarningAndProceedsToNext()
    {
        // Arrange
        List<ValidationError> errors =
        [
            new ValidationError("Name", "Name is required")
        ];

        ValidationResult validationResult = new()
        {
            IsValid = false,
            Errors = errors
        };

        Mock<IValidator<TestRequest>> validator = new();
        validator.Setup(v => v.Validate(It.IsAny<TestRequest>()))
            .Returns(validationResult);

        IEnumerable<IValidator<TestRequest>> serviceCollection = [validator.Object];

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IValidator<TestRequest>>)))
            .Returns(serviceCollection);

        ValidationOptions options = new()
        {
            LogValidationFailures = true,
            ThrowExceptionOnValidationFailure = false
        };

        ValidationBehavior<TestRequest, string> behavior = new(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            options);

        TestRequest request = new();
        string expectedResult = "Success despite validation failure";

        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>(expectedResult);

        // Act
        string result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);

        // Verify logging was called
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validation failed for request")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ),
            Times.Once
        );
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    }

    public class TestRequest : IRequest<string>
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }
}