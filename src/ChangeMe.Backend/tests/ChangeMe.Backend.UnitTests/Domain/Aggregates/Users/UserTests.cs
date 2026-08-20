using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.UnitTests;

public sealed class UserTests
{
  [Fact]
  public void Create_WhenInputIsValid_ShouldTrimValuesAndSetAccountFields()
  {
    var result = User.Create("  John  ", "  Doe  ", " Test@Example.com ", "  hashed-password  ");

    Assert.True(result.IsSuccess);
    var user = result.Value;
    Assert.Equal("John", user.FirstName);
    Assert.Equal("Doe", user.LastName);
    Assert.Equal("Test@Example.com", user.Email);
    Assert.Equal("TEST@EXAMPLE.COM", user.NormalizedEmail);
    Assert.Equal("hashed-password", user.PasswordHash);
    Assert.False(user.Deactivated);
    Assert.True(user.IsActive);
    Assert.Equal("John Doe (Test@Example.com)", user.DisplayLabel);
  }

  [Fact]
  public void DisplayLabel_WhenProfileIsComplete_ShouldIncludeNameAndEmail()
  {
    var user = User.Create("Jan", "Kowalski", "jan@example.com", "hash").Value;

    Assert.Equal("Jan Kowalski (jan@example.com)", user.DisplayLabel);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("invalid-email")]
  public void Create_WhenEmailIsInvalid_ShouldReturnInvalidResult(string email)
  {
    var result = User.Create("John", "Doe", email, "hashed-password");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void Create_WhenNamesAreMissing_ShouldReturnInvalidResult()
  {
    var result = User.Create("", " ", "john@example.com", "hashed-password");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void Create_WhenPasswordHashIsMissing_ShouldReturnInvalidResult()
  {
    var result = User.Create("John", "Doe", "john@example.com", " ");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(User.PasswordHash));
  }

  [Fact]
  public void Deactivate_WhenCalled_ShouldSetDeactivatedAt()
  {
    var user = User.Create("John", "Doe", "john@example.com", "hash").Value;

    user.Deactivate();

    Assert.True(user.Deactivated);
    Assert.NotNull(user.DeactivatedAt);
    Assert.False(user.IsActive);
  }

  [Fact]
  public void Activate_WhenCalled_ShouldClearDeactivatedAt()
  {
    var user = User.Create("John", "Doe", "john@example.com", "hash").Value;
    user.Deactivate();

    user.Activate();

    Assert.False(user.Deactivated);
    Assert.Null(user.DeactivatedAt);
    Assert.True(user.IsActive);
  }

  [Theory]
  [InlineData(" Test@Example.com ", "TEST@EXAMPLE.COM")]
  [InlineData("user@example.com", "USER@EXAMPLE.COM")]
  public void NormalizeEmail_WhenCalled_ShouldTrimAndUppercaseValue(string email, string expected)
  {
    var result = User.NormalizeEmail(email);

    Assert.Equal(expected, result);
  }

  [Fact]
  public void UpdateProfile_WhenInputIsValid_ShouldTrimValues()
  {
    var user = User.Create("John", "Doe", "john@example.com", "hash").Value;

    var result = user.UpdateProfile("  Jane  ", "  Smith  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("Jane", user.FirstName);
    Assert.Equal("Smith", user.LastName);
  }

  [Fact]
  public void SetPasswordHash_WhenHashIsValid_ShouldUpdatePasswordHash()
  {
    var user = User.Create("John", "Doe", "john@example.com", "old-hash").Value;

    var result = user.SetPasswordHash("new-hash");

    Assert.True(result.IsSuccess);
    Assert.Equal("new-hash", user.PasswordHash);
  }

  [Fact]
  public void SetPasswordHash_WhenHashIsMissing_ShouldReturnInvalidResult()
  {
    var user = User.Create("John", "Doe", "john@example.com", "hash").Value;

    var result = user.SetPasswordHash(" ");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }
}
