using Template.Backend.Infrastructure.Configurations;
using Template.Backend.Web.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

var loggerFactory = LoggerFactory.Create(lb => lb.AddSimpleConsole(o => o.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddCors(builder);
builder.Services.AddRateLimiting(builder);
builder.Services.AddJwtAuthentication(builder);

builder.Services.AddHttpContextAccessor();

builder.Services.AddDatabase(builder, logger);
builder.Services.AddHangfire(builder, logger);
builder.Services.AddInfrastructureServices(builder.Configuration, logger);
builder.Services.AddApplicationMediator();
builder.Services.AddNotifications(builder, logger);
builder.Services.AddFileStorage(builder, logger);

logger.LogInformation("Starting web host");

builder.Services.AddFastEndpointsWithSwagger(builder.Configuration);

var app = builder.Build();

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
