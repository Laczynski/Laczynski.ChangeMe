using Hangfire;
using Microsoft.Extensions.Options;
using Template.Backend.Infrastructure.FileStorage;

namespace Template.Backend.Web.Configurations;

public static class FileStorageConfig
{
  public static IServiceCollection AddFileStorage(this IServiceCollection services, WebApplicationBuilder builder, Microsoft.Extensions.Logging.ILogger logger)
  {
    services.AddScoped<AttachmentStorageCleanupJob>();

    logger.LogInformation("{Project} services configured", "FileStorage");
    return services;
  }

  public static WebApplication UseFileStorageCleanup(this WebApplication app)
  {
    var fileStorageOptions = app.Services.GetRequiredService<IOptions<FileStorageOptions>>().Value;
    var cleanupCronExpression = string.IsNullOrWhiteSpace(fileStorageOptions.CleanupCronExpression)
      ? "0 * * * *"
      : fileStorageOptions.CleanupCronExpression;

    var cleanupConcurrentExecutionTimeoutSeconds = fileStorageOptions.CleanupConcurrentExecutionTimeoutSeconds;
    if (cleanupConcurrentExecutionTimeoutSeconds <= 0)
      cleanupConcurrentExecutionTimeoutSeconds = 3600;

    GlobalJobFilters.Filters.Add(
      new AttachmentStorageCleanupConcurrentExecutionFilterAttribute(cleanupConcurrentExecutionTimeoutSeconds));

    var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();
    recurringJobs.AddOrUpdate<AttachmentStorageCleanupJob>(
      "attachment-storage-cleanup",
      job => job.ExecuteAsync(JobCancellationToken.Null),
      cleanupCronExpression);

    return app;
  }
}
