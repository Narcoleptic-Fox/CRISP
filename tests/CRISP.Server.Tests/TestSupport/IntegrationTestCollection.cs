namespace CRISP.Server.Tests.TestSupport;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Integration";
}
