using CRISP.Core.Items;

namespace CRISP.Core.Tests;

public sealed class ItemValidatorTests
{
    [Fact]
    public void CreateItemValidator_WithValidCommand_IsValid()
    {
        var command = new CreateItem
        {
            Name = "Widget",
            Description = "A standard widget",
            Owner = "Ada Lovelace",
        };

        var result = new CreateItemValidator().TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateItemValidator_WithBlankName_ReturnsExpectedError()
    {
        var command = new CreateItem { Name = string.Empty };

        var result = new CreateItemValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(CreateItem.Name)).WithErrorMessage("Item name is required.");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void UpdateItemValidator_WithMissingId_ReturnsExpectedError()
    {
        var command = new UpdateItem { Name = "Widget" };

        var result = new UpdateItemValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(UpdateItem.Id)).WithErrorMessage("Item ID is required.");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void UpdateItemValidator_WithNameOverMaximumLength_ReturnsExpectedError()
    {
        var command = new UpdateItem
        {
            Id = Guid.NewGuid(),
            Name = new string('x', 101),
        };

        var result = new UpdateItemValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(UpdateItem.Name)).WithErrorMessage("Item name must not exceed 100 characters.");
        result.IsValid.ShouldBeFalse();
    }
}
