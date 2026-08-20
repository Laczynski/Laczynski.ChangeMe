using ChangeMe.Backend.Infrastructure.Email;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Email;

public sealed class BrandedEmailTemplatesTests
{
  [Fact]
  public void BuildActionEmail_ShouldIncludeBrandingAndActionLink()
  {
    var html = BrandedEmailTemplates.BuildActionEmail(
      "Reset your password",
      "Summary text.",
      "Detail text.",
      "https://app.example.com/reset-password?token=abc",
      "Reset password");

    Assert.Contains("ChangeMe", html, StringComparison.Ordinal);
    Assert.Contains("#10b981", html, StringComparison.Ordinal);
    Assert.Contains("https://app.example.com/reset-password?token=abc", html, StringComparison.Ordinal);
    Assert.Contains("Reset password", html, StringComparison.Ordinal);
    Assert.Contains("Summary text.", html, StringComparison.Ordinal);
  }
}
