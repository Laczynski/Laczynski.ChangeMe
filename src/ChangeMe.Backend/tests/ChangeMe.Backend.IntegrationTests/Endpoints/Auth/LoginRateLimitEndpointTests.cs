using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.Web.Configurations;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class LoginRateLimitEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostLogin_WhenRateLimitExceeded_ShouldReturnTooManyRequests()
  {
    const int permitLimit = 2;
    var cancellationToken = TestContext.Current.CancellationToken;
    var authPermitLimitKey = $"{RateLimitingOptions.SectionName}__AuthPermitLimit";
    var previousPermitLimit = Environment.GetEnvironmentVariable(authPermitLimitKey);

    Environment.SetEnvironmentVariable(authPermitLimitKey, permitLimit.ToString());

    try
    {
      using var rateLimitedFactory = factory.WithWebHostBuilder(_ => { });
      using var client = rateLimitedFactory.CreateClient(new WebApplicationFactoryClientOptions
      {
        BaseAddress = new Uri("https://localhost")
      });

      HttpResponseMessage? lastResponse = null;

      for (var i = 0; i <= permitLimit; i++)
      {
        lastResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
          Email = "rate-limit@example.com",
          Password = "WrongPass123!"
        }, cancellationToken);
      }

      Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse!.StatusCode);
      Assert.True(lastResponse.Headers.Contains("Retry-After"));
    }
    finally
    {
      Environment.SetEnvironmentVariable(authPermitLimitKey, previousPermitLimit);
    }
  }
}
