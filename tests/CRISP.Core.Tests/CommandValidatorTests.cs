using CRISP.Core.Common;

namespace CRISP.Core.Tests;

public sealed class CommandValidatorTests
{
    [Fact]
    public void ArchiveCommandValidator_WithMissingId_ReturnsExpectedError()
    {
        var command = new ArchiveCommand(Guid.Empty, "Duplicate record");

        var result = new ArchiveCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(ArchiveCommand.Id)).WithErrorMessage("Entity ID is required.");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ArchiveCommandValidator_WithReasonOverMaximumLength_ReturnsExpectedError()
    {
        var command = new ArchiveCommand(Guid.NewGuid(), new string('x', 501));

        var result = new ArchiveCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(ArchiveCommand.ArchiveReason)).WithErrorMessage("Archive reason must not exceed 500 characters.");
        result.IsValid.ShouldBeFalse();
    }
}