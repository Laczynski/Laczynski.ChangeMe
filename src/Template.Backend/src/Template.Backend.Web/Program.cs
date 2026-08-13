using Template.Backend.Infrastructure.Configurations;
using Template.Backend.Web.Configurations;

LocalEnvironmentLoader.LoadForDevelopment(args);

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

var loggerFactory = LoggerFactory.Create(lb => lb.AddSimpleConsole(o => o.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddRuntimeConfiguration(builder.Configuration);
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
