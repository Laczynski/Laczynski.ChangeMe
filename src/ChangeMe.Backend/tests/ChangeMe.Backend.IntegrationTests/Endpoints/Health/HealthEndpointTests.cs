using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class HealthEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetHealth_WhenApplicationStarts_ShouldReturnSuccessStatusCode()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.GetAsync("/health", cancellationToken);

    Assert.True(response.IsSuccessStatusCode);
  }
}
