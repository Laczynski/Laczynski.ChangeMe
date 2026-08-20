namespace ChangeMe.Backend.Domain.Common;

public interface IUserAccessor
{
  Guid? UserId { get; }
  Guid? SessionId { get; }
  bool HasPermission(string permissionCode);
}
