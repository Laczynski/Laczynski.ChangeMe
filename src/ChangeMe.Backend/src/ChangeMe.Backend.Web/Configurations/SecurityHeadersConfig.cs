namespace ChangeMe.Backend.Web.Configurations;

public static class SecurityHeadersConfig
{
  public static WebApplication UseSecurityHeaders(this WebApplication app)
  {
    app.Use(async (context, next) =>
    {
      var headers = context.Response.Headers;

      headers.XContentTypeOptions = "nosniff";
      headers.XFrameOptions = "DENY";
      headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
      headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
      headers["Cross-Origin-Opener-Policy"] = "same-origin";
      headers["Cross-Origin-Resource-Policy"] = "same-origin";
      headers["Cross-Origin-Embedder-Policy"] = "unsafe-none";

      await next();
    });

    return app;
  }
}
