using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.UnitTests.Domain.Aggregates.Roles;

public sealed class RoleTests
{
  [Fact]
  public void Create_WhenInputIsValid_ShouldTrimValues()
  {
    var result = Role.Create("  Custom  ", "  Description  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("Custom", result.Value.Name);
    Assert.Equal("Description", result.Value.Description);
    Assert.False(result.Value.IsSystem);
  }

  [Theory]
  [InlineData("A")]
  [InlineData(" ")]
  public void Create_WhenNameIsTooShort_ShouldReturnInvalidResult(string name)
  {
    var result = Role.Create(name, null);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void Create_WhenNameExceedsMaxLength_ShouldReturnInvalidResult()
  {
    var result = Role.Create(new string('R', RoleConstraints.NAME_MAX_LENGTH + 1), null);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void Create_WhenDescriptionExceedsMaxLength_ShouldReturnInvalidResult()
  {
    var result = Role.Create("Valid", new string('D', RoleConstraints.DESCRIPTION_MAX_LENGTH + 1));

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void CreateAdministrator_ShouldCreateSystemRoleWithAllPermissions()
  {
    var role = Role.CreateAdministrator();

    Assert.Equal(RoleConstraints.AdministratorRoleName, role.Name);
    Assert.Equal(RoleConstraints.AdministratorDescription, role.Description);
    Assert.True(role.IsSystem);
    Assert.Equal(PermissionCodes.All.Count, role.Permissions.Count);
  }

  [Fact]
  public void CreateDefaultUserRole_ShouldCreateSystemRoleWithDefaultPermissions()
  {
    var role = Role.CreateDefaultUserRole();

    Assert.Equal(RoleConstraints.UserRoleName, role.Name);
    Assert.Equal(RoleConstraints.UserDescription, role.Description);
    Assert.True(role.IsSystem);
    Assert.Equal(PermissionCodes.DefaultUserRole.Count, role.Permissions.Count);
  }

  [Fact]
  public void UpdateProfile_WhenCustomRole_ShouldUpdateFields()
  {
    var role = Role.Create("Original", "Old").Value;

    var result = role.UpdateProfile("  Updated  ", "  New description  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("Updated", role.Name);
    Assert.Equal("New description", role.Description);
  }

  [Fact]
  public void UpdateProfile_WhenSystemRole_ShouldReturnError()
  {
    var role = Role.CreateAdministrator();

    var result = role.UpdateProfile("Renamed", null);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Error, result.Status);
  }

  [Fact]
  public void SetPermissions_ShouldReplacePermissionCodes()
  {
    var role = Role.Create("Permissions", null).Value;

    role.SetPermissions(["Users.View", "Roles.View", "Users.View"]);

    Assert.Equal(2, role.Permissions.Count);
    Assert.Contains(role.Permissions, x => x.PermissionCode == "Users.View");
    Assert.Contains(role.Permissions, x => x.PermissionCode == "Roles.View");
  }

  [Fact]
  public void NormalizeName_ShouldUppercaseTrimmedValue()
  {
    Assert.Equal("CUSTOM ROLE", Role.NormalizeName("  custom role  "));
  }
}
