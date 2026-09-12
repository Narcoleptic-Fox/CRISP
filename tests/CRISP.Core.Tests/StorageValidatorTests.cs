using CRISP.Core.Storage;

namespace CRISP.Core.Tests;

public sealed class StorageValidatorTests
{
    [Fact]
    public void CreatePresignedUploadUrlValidator_WithValidRequest_IsValid()
    {
        var command = new CreatePresignedUploadUrl
        {
            EntityType = "Item",
            EntityId = Guid.NewGuid(),
            OriginalFileName = "document.pdf",
            ContentType = "application/pdf",
            SizeBytes = 1024,
            ExpiresInMinutes = 15,
        };

        var result = new CreatePresignedUploadUrlValidator().TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(61)]
    public void CreatePresignedUploadUrlValidator_WithExpirationOutsideRange_ReturnsExpectedError(int expiresInMinutes)
    {
        var command = new CreatePresignedUploadUrl
        {
            EntityType = "Item",
            EntityId = Guid.NewGuid(),
            OriginalFileName = "document.pdf",
            ContentType = "application/pdf",
            SizeBytes = 1024,
            ExpiresInMinutes = expiresInMinutes,
        };

        var result = new CreatePresignedUploadUrlValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(CreatePresignedUploadUrl.ExpiresInMinutes)).WithErrorMessage("Expiration must be between 1 and 60 minutes.");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void CreatePresignedUploadUrlValidator_WithZeroSize_ReturnsExpectedError()
    {
        var command = new CreatePresignedUploadUrl
        {
            EntityType = "Item",
            EntityId = Guid.NewGuid(),
            OriginalFileName = "document.pdf",
            ContentType = "application/pdf",
            SizeBytes = 0,
            ExpiresInMinutes = 15,
        };

        var result = new CreatePresignedUploadUrlValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(CreatePresignedUploadUrl.SizeBytes)).WithErrorMessage("File size must be greater than zero.");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void CreatePresignedDownloadUrlValidator_WithEmptyFileId_ReturnsExpectedError()
    {
        var command = new CreatePresignedDownloadUrl
        {
            EntityType = "Item",
            EntityId = Guid.NewGuid(),
            FileId = Guid.Empty,
            ExpiresInMinutes = 15,
        };

        var result = new CreatePresignedDownloadUrlValidator().TestValidate(command);
        result.ShouldHaveValidationErrors();
        result.ShouldHaveValidationErrorFor(nameof(CreatePresignedDownloadUrl.FileId)).WithErrorMessage("File ID is required.");
        result.IsValid.ShouldBeFalse();
    }
}
