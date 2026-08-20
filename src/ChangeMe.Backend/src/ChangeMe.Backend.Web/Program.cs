using ChangeMe.Backend.Infrastructure.Configurations;
using ChangeMe.Backend.Web.Configurations;
using Microsoft.AspNetCore.HttpOverrides;

var startupOptions = ApplicationStartupOptions.Parse(args);

LocalEnvironmentLoader.LoadForDevelopment(startupOptions.ConfigurationArguments);

var builder = WebApplication.CreateBuilder(startupOptions.ConfigurationArguments);

builder.AddSerilog();

var loggerFactory = LoggerFactory.Create(lb => lb.AddSimpleConsole(o => o.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddRuntimeConfiguration(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
  options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
builder.Services.AddCors(builder);
builder.Services.AddRateLimiting();
builder.Services.AddJwtAuthentication(builder);

builder.Services.AddHttpContextAccessor();

builder.Services.AddDatabase(builder, logger);
builder.Services.AddHangfire(builder, logger);
builder.Services.AddInfrastructureServices(logger);
builder.Services.AddApplicationMediator();
builder.Services.AddNotifications(logger);
builder.Services.AddFileStorage(logger);

logger.LogInformation("Starting web host");

builder.Services.AddFastEndpointsWithSwagger(builder.Configuration);

var app = builder.Build();

app.ValidateRuntimeConfiguration();

if (startupOptions.MigrateOnly)
{
  try
  {
    logger.LogInformation("Running database migrations and bootstrap seed without starting the web host");
    await DatabaseConfig.InitializeDatabaseAsync(app.Services, CancellationToken.None);
    logger.LogInformation("Database migrations and bootstrap seed completed");
  }
  finally
  {
    await app.DisposeAsync();
  }

  return;
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseSecurityHeaders();

app.UseCors(CorsConfig.CorsPolicyName);
app.UseRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpointsWithSwagger();
app.UseHangfireDashboard();

app.MapHealthChecks("/health");
app.UseNotifications();
app.UseFileStorageCleanup();

await app.UseDatabase();

await app.RunAsync();

public partial class Program
{
  protected Program() { }
}
