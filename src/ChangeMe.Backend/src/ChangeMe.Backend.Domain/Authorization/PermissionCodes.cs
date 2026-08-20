namespace ChangeMe.Backend.Domain.Authorization;

public static class PermissionCodes
{
  public const string UsersView = "Users.View";
  public const string UsersManage = "Users.Manage";
  public const string UsersDeactivate = "Users.Deactivate";
  public const string RolesView = "Roles.View";
  public const string RolesManage = "Roles.Manage";
  public const string SessionsViewOwn = "Sessions.ViewOwn";
  public const string SessionsManageOwn = "Sessions.ManageOwn";
  public const string SessionsViewAny = "Sessions.ViewAny";
  public const string SessionsManageAny = "Sessions.ManageAny";

  public static readonly IReadOnlyList<string> All =
  [
    UsersView,
    UsersManage,
    UsersDeactivate,
    RolesView,
    RolesManage,
    SessionsViewOwn,
    SessionsManageOwn,
    SessionsViewAny,
    SessionsManageAny
  ];

  public static readonly IReadOnlyList<string> DefaultUserRole =
  [
    SessionsViewOwn,
    SessionsManageOwn
  ];
}
