using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class PasswordPolicyValidatorTests
{
  private readonly PasswordPolicyValidator validator = new(Options.Create(new AuthOptions
  {
    PasswordPolicy = new PasswordPolicyOptions
    {
      MinimumLength = 10,
      MaximumLength = 20,
      RequireUppercase = true,
      RequireLowercase = true,
      RequireDigit = true,
      RequireSpecialCharacter = true
    }
  }));

  [Fact]
  public void Validate_WhenPasswordMeetsPolicy_ShouldReturnNoErrors()
  {
    var errors = validator.Validate("Aa1!aaaaaa");

    Assert.Empty(errors);
  }

  [Fact]
  public void Validate_WhenTooShort_ShouldReturnMinimumLengthMessage()
  {
    var errors = validator.Validate("Aa1!");

    Assert.Contains(errors, e => e.ErrorMessage == "Password must be at least 10 characters.");
  }

  [Fact]
  public void Validate_WhenMissingUppercase_ShouldReturnUppercaseMessage()
  {
    var errors = validator.Validate("aa1!aaaaaa");

    Assert.Contains(errors, e => e.ErrorMessage == "Password must contain at least one uppercase letter.");
  }

  [Fact]
  public void Validate_WhenMissingSpecialCharacter_ShouldReturnSpecialCharacterMessage()
  {
    var errors = validator.Validate("Aa1aaaaaaa");

    Assert.Contains(errors, e => e.ErrorMessage == "Password must contain at least one special character.");
  }

  [Fact]
  public void Validate_WhenEmpty_ShouldReturnNoErrors()
  {
    var errors = validator.Validate(string.Empty);

    Assert.Empty(errors);
  }
}
