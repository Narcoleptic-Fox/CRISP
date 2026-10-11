using CRISP.Server.Common;

namespace CRISP.Server.Tests.Common;

[Trait("Category", "Unit")]
public class ObjectStorageKeyTests
{
    [Theory]
    [InlineData("items/2026/doc.pdf", "items/2026/doc.pdf")]
    [InlineData("  items/doc.pdf  ", "items/doc.pdf")]
    [InlineData("items\\2026\\doc.pdf", "items/2026/doc.pdf")]
    [InlineData("items//2026///doc.pdf", "items/2026/doc.pdf")]
    [InlineData("/items/doc.pdf", "items/doc.pdf")]
    public void Normalize_CanonicalizesSeparatorsAndLeadingSlashes(string input, string expected)
    {
        ObjectStorageKey.Normalize(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_Rejects_BlankKeys(string input)
    {
        Should.Throw<RejectedException>(() => ObjectStorageKey.Normalize(input));
    }

    [Theory]
    [InlineData("../secrets.txt")]
    [InlineData("items/../../secrets.txt")]
    [InlineData("items/./doc.pdf")]
    [InlineData("/")]
    public void Normalize_Rejects_TraversalSegments(string input)
    {
        // Traversal must be rejected after normalization, otherwise a key could
        // escape the configured bucket prefix.
        Should.Throw<RejectedException>(() => ObjectStorageKey.Normalize(input));
    }

    [Fact]
    public void Normalize_Rejects_KeysOverTheProviderCeiling()
    {
        var tooLong = new string('a', 1025);

        Should.Throw<RejectedException>(() => ObjectStorageKey.Normalize(tooLong));
    }
}
