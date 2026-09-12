using CRISP.Core.Identity;

namespace CRISP.Core.Tests;

public sealed class IdentityValidatorTests
{
    [Fact]
    public void CreateUserValidator_WithValidUser_IsValid()
    {
        var command = new CreateUser
        {
            Username = "apptemplate.admin",
            Email = "admin@example.com",
            Role = Roles.Admin,
            IsActive = true,
        };

        var result = new CreateUserValidator().TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateUserValidator_WithInvalidEmail_ReturnsExpectedError()
    {
        var command = new CreateUser
        {
            Username = "apptemplate.admin",
            Email = "not-an-email",
            Role = Roles.Admin,
            IsActive = true,
        };

        var result = new CreateUserValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(CreateUser.Email)).WithErrorMessage("Invalid email format.");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void CreateUserValidator_WithInvalidRole_ReturnsExpectedError()
    {
        var command = new CreateUser
        {
            Username = "apptemplate.admin",
            Email = "admin@example.com",
            Role = (Roles)999,
            IsActive = true,
        };

        var result = new CreateUserValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(CreateUser.Role)).WithErrorMessage("Invalid role specified.");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void UpdateUserValidator_WithMissingId_ReturnsExpectedError()
    {
        var command = new UpdateUser
        {
            Username = "apptemplate.editor",
            Email = "editor@example.com",
            Role = Roles.Editor,
            IsActive = true,
        };

        var result = new UpdateUserValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(UpdateUser.Id)).WithErrorMessage("User ID is required.");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void UpdateUserSettingsValidator_WithDefaultSettings_IsValid()
    {
        var command = new UpdateUserSettings
        {
            Id = Guid.NewGuid(),
        };

        var result = new UpdateUserSettingsValidator().TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateUserSettingsValidator_WithMissingId_ReturnsExpectedError()
    {
        var command = new UpdateUserSettings();

        var result = new UpdateUserSettingsValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(UpdateUserSettings.Id)).WithErrorMessage("User ID is required.");
        result.IsValid.ShouldBeFalse();
    }
}