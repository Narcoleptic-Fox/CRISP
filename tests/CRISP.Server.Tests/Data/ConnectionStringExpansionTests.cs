using CRISP.Server.Data;

namespace CRISP.Server.Tests.Data;

[Trait("Category", "Unit")]
public sealed class ConnectionStringExpansionTests
{
    private static Func<string, string?> Env(params (string Name, string Value)[] values) =>
        name => values.FirstOrDefault(v => v.Name == name) is { Value: { } value } ? value : null;

    // The committed development settings carry Password=${DBPASS}; the secret arrives from the
    // environment at startup and never from the file.
    [Fact]
    public void ExpandsATokenFromTheEnvironment()
    {
        ConnectionStringExpansion
            .Expand("Host=db;Password=${DBPASS};", Env(("DBPASS", "s3cret")))
            .ShouldBe("Host=db;Password=s3cret;");
    }

    [Fact]
    public void ExpandsEveryToken()
    {
        ConnectionStringExpansion
            .Expand("Host=${DBHOST};Password=${DBPASS};", Env(("DBHOST", "db.internal"), ("DBPASS", "x")))
            .ShouldBe("Host=db.internal;Password=x;");
    }

    [Fact]
    public void LeavesAStringWithoutTokensAlone()
    {
        const string plain = "Host=db;Password=literal;";
        ConnectionStringExpansion.Expand(plain, Env()).ShouldBe(plain);
    }

    // A literal "${DBPASS}" sent to the server would fail later as an authentication error, which
    // points at the wrong thing. Fail here and name the variable.
    [Fact]
    public void FailsNamingTheMissingVariable()
    {
        var error = Should.Throw<InvalidOperationException>(() =>
            ConnectionStringExpansion.Expand("Host=db;Password=${DBPASS};", Env()));

        error.Message.ShouldContain("DBPASS");
    }

    [Fact]
    public void DoesNotTreatADollarWithoutBracesAsAToken()
    {
        const string plain = "Host=db;Password=pa$$word;";
        ConnectionStringExpansion.Expand(plain, Env()).ShouldBe(plain);
    }
}
