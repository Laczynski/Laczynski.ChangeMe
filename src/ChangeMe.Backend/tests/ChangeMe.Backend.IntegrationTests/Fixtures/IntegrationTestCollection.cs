namespace ChangeMe.Backend.IntegrationTests.Fixtures;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<BackendWebApplicationFactory>
{
  public const string Name = "Backend integration tests";
}
