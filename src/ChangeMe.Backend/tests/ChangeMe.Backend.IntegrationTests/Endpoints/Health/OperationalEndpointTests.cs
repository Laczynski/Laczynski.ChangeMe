using System.Net;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Health;

[Collection(IntegrationTestCollection.Name)]
public sealed class OperationalEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetSwagger_WhenDisabledInTesting_ShouldReturnNotFound()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.GetAsync("/swagger", cancellationToken);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetHangfireDashboard_WhenDisabledInTesting_ShouldReturnNotFound()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.GetAsync("/hangfire", cancellationToken);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}
