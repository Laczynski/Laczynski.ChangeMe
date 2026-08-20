using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.UnitTests.Domain.Authorization;

public sealed class PermissionCatalogTests
{
  [Fact]
  public void All_ShouldContainEveryPermissionCode()
  {
    Assert.Equal(PermissionCodes.All.Count, PermissionCatalog.All.Count);
    Assert.All(PermissionCodes.All, code =>
      Assert.Contains(PermissionCatalog.All, definition => definition.Code == code));
  }

  [Fact]
  public void BuildEffectivePermissions_ShouldUnionByCodeAndCollectRoleNames()
  {
    var grants = new[]
    {
      (PermissionCodes.UsersView, "Administrator"),
      (PermissionCodes.UsersView, "Custom"),
      (PermissionCodes.RolesView, "Custom")
    };

    var effective = PermissionCatalog.BuildEffectivePermissions(grants);

    Assert.Equal(2, effective.Count);
    var usersView = effective.Single(x => x.Code == PermissionCodes.UsersView);
    Assert.Equal(["Administrator", "Custom"], usersView.FromRoleNames);
    Assert.Equal("View users", usersView.Label);
  }

  [Fact]
  public void Find_WhenCodeIsUnknown_ShouldReturnNull()
  {
    Assert.Null(PermissionCatalog.Find("Unknown.Permission"));
  }
}
