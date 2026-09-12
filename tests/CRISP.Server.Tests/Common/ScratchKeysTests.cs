using Microsoft.AspNetCore.Http;
using CRISP.Server.Common;

namespace CRISP.Server.Tests.Common;

/// <summary>
/// The ownership rule encoded in the scratch key layout.
/// </summary>
/// <remarks>
/// A scratch key names an object in a bucket shared by every user, and the client hands it back to
/// attach it. These are the cases that decide whether accepting it is safe.
/// </remarks>
[Trait("Category", "Unit")]
public class ScratchKeysTests
{
    private static readonly Guid Owner = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Other = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void AKeyIssuedToAUser_BelongsToThem()
    {
        var key = ScratchKeys.For(Owner, ".pdf");

        ScratchKeys.BelongTo(key, Owner).ShouldBeTrue();
    }

    [Fact]
    public void AKeyIssuedToSomeoneElse_DoesNot()
    {
        var key = ScratchKeys.For(Other, ".pdf");

        ScratchKeys.BelongTo(key, Owner).ShouldBeFalse();
    }

    [Fact]
    public void EachKeyIsDistinct_SoTwoPicksCannotOverwriteEachOther()
    {
        ScratchKeys.For(Owner, ".pdf").ShouldNotBe(ScratchKeys.For(Owner, ".pdf"));
    }

    [Fact]
    public void TheExtensionIsKept_SoTheStoredObjectStaysRecognisable()
    {
        ScratchKeys.For(Owner, ".pdf").ShouldEndWith(".pdf");
    }

    // Keys are normalized before the prefix is compared, so dressing another user's prefix up with
    // traversal or padding does not get past it.
    [Theory]
    [InlineData("scratch/22222222-2222-2222-2222-222222222222/x.pdf")]
    [InlineData("items/11111111-1111-1111-1111-111111111111/files/x.pdf")]
    [InlineData("scratch/11111111-1111-1111-1111-111111111111x/x.pdf")]
    [InlineData("Scratch/11111111-1111-1111-1111-111111111111/x.pdf")]
    public void AKeyThatIsNotTheirs_IsRejected(string key)
    {
        ScratchKeys.BelongTo(key, Owner).ShouldBeFalse();
    }

    [Theory]
    [InlineData("scratch/../scratch/11111111-1111-1111-1111-111111111111/x.pdf")]
    [InlineData("scratch/11111111-1111-1111-1111-111111111111/../../etc/x.pdf")]
    public void ATraversingKey_IsRefusedOutright(string key)
    {
        // Normalization throws rather than quietly resolving, so a traversal attempt is a bad
        // request rather than a key that silently means something else.
        Should.Throw<RejectedException>(() => ScratchKeys.BelongTo(key, Owner))
            .StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    // Leading and duplicated separators normalize away, so the same key written loosely is still
    // recognised as its owner's rather than being rejected for a formatting difference.
    [Theory]
    [InlineData("/scratch/11111111-1111-1111-1111-111111111111/x.pdf")]
    [InlineData("scratch//11111111-1111-1111-1111-111111111111/x.pdf")]
    public void ALooselyWrittenKey_StillBelongsToItsOwner(string key)
    {
        ScratchKeys.BelongTo(key, Owner).ShouldBeTrue();
    }
}
