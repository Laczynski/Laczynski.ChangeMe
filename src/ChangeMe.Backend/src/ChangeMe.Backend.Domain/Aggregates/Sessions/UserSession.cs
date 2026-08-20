namespace ChangeMe.Backend.Domain.Aggregates.Sessions;

public class UserSession : Entity, IAggregateRoot
{
  private UserSession() { }

  public Guid UserId { get; private set; }
  public DateTime SignedInAt { get; private set; }
  public DateTime LastActivityAt { get; private set; }
  public string DeviceBrowserLabel { get; private set; } = string.Empty;
  public string SignInMethod { get; private set; } = SignInMethods.Password;
  public string? IpAddress { get; private set; }
  public string RefreshTokenHash { get; private set; } = string.Empty;
  public DateTime RefreshTokenExpiresAtUtc { get; private set; }
  public DateTime? RevokedAt { get; private set; }

  public bool IsRevoked => RevokedAt is not null;

  public static Result<UserSession> Create(
    Guid userId,
    string deviceBrowserLabel,
    string? ipAddress,
    string refreshTokenHash,
    DateTime refreshTokenExpiresAtUtc,
    DateTime signedInAtUtc,
    string signInMethod = SignInMethods.Password)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (string.IsNullOrWhiteSpace(deviceBrowserLabel))
      validationErrors.Add(new ValidationError(nameof(DeviceBrowserLabel), "cannot be null or empty"));
    else if (deviceBrowserLabel.Length > SessionConstraints.DEVICE_LABEL_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(DeviceBrowserLabel), $"cannot be longer than {SessionConstraints.DEVICE_LABEL_MAX_LENGTH} characters"));

    if (ipAddress is not null && ipAddress.Length > SessionConstraints.IP_ADDRESS_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(IpAddress), $"cannot be longer than {SessionConstraints.IP_ADDRESS_MAX_LENGTH} characters"));

    if (string.IsNullOrWhiteSpace(signInMethod))
      validationErrors.Add(new ValidationError(nameof(SignInMethod), "cannot be null or empty"));
    else if (signInMethod.Length > SessionConstraints.SIGN_IN_METHOD_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(SignInMethod), "is too long"));

    if (string.IsNullOrWhiteSpace(refreshTokenHash))
      validationErrors.Add(new ValidationError(nameof(RefreshTokenHash), "cannot be null or empty"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new UserSession
    {
      UserId = userId,
      SignedInAt = signedInAtUtc,
      LastActivityAt = signedInAtUtc,
      DeviceBrowserLabel = deviceBrowserLabel.Trim(),
      SignInMethod = signInMethod.Trim(),
      IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim(),
      RefreshTokenHash = refreshTokenHash,
      RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
      CreatedBy = userId,
      UpdatedBy = userId
    });
  }

  public bool IsActive(DateTime utcNow, int sessionLifetimeDays) =>
    !IsRevoked && SignedInAt.AddDays(sessionLifetimeDays) > utcNow;

  public void TouchActivity(DateTime utcNow) => LastActivityAt = utcNow;

  public void Revoke(DateTime utcNow) => RevokedAt = utcNow;

  public void RotateRefreshToken(string refreshTokenHash, DateTime refreshTokenExpiresAtUtc)
  {
    RefreshTokenHash = refreshTokenHash;
    RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc;
    TouchActivity(DateTime.UtcNow);
  }
}

public static class SessionConstraints
{
  public const int DEVICE_LABEL_MAX_LENGTH = 200;
  public const int IP_ADDRESS_MAX_LENGTH = 64;
  public const int SIGN_IN_METHOD_MAX_LENGTH = 128;
  public const int REFRESH_TOKEN_BYTES = 32;
  public const int ACCESS_TOKEN_LIFETIME_MINUTES = 30;
}
