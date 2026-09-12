using CRISP.Server.Data.Interceptors;

namespace CRISP.Server.Tests.Data;

[Trait("Category", "Unit")]
public class AuditLogInterceptorDescribeTests
{
    // The bug this exists to stop: a List<string> stored through a JSON value converter rendered as
    // "System.Collections.Generic.List`1[System.String]" in every activity row that touched it.
    [Fact]
    public void Describe_RendersAStringCollectionAsItsValues()
    {
        AuditLogInterceptor.Describe(new List<string> { "first", "second" }).ShouldBe("first, second");
    }

    [Fact]
    public void Describe_RendersAnEmptyCollectionAsEmpty()
    {
        AuditLogInterceptor.Describe(new List<int>()).ShouldBe(string.Empty);
    }

    [Fact]
    public void Describe_CountsBytesRatherThanListingThem()
    {
        AuditLogInterceptor.Describe(new byte[] { 1, 2, 3 }).ShouldBe("3 bytes");
    }

    [Fact]
    public void Describe_DoesNotTreatAStringAsACollectionOfChars()
    {
        AuditLogInterceptor.Describe("plain").ShouldBe("plain");
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData(42, "42")]
    [InlineData(true, "True")]
    public void Describe_FallsBackToToString(object? value, string expected)
    {
        AuditLogInterceptor.Describe(value).ShouldBe(expected);
    }

    [Fact]
    public void Describe_RendersNullElementsInsideACollection()
    {
        AuditLogInterceptor.Describe(new string?[] { "a", null }).ShouldBe("a, null");
    }
}
