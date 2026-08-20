using ChangeMe.Backend.Infrastructure.FileStorage;
using Hangfire;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Web.Configurations;

public static class FileStorageConfig
{
  public static IServiceCollection AddFileStorage(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
  {
    services.AddScoped<AttachmentStorageCleanupJob>();

    logger.LogInformation("{Project} services configured", "FileStorage");
    return services;
  }

  public static WebApplication UseFileStorageCleanup(this WebApplication app)
  {
    var fileStorageOptions = app.Services.GetRequiredService<IOptions<FileStorageOptions>>().Value;
    var cleanupConcurrentExecutionTimeoutSeconds = fileStorageOptions.CleanupConcurrentExecutionTimeoutSeconds;

    GlobalJobFilters.Filters.Add(
      new AttachmentStorageCleanupConcurrentExecutionFilterAttribute(cleanupConcurrentExecutionTimeoutSeconds));

    var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();
    recurringJobs.AddOrUpdate<AttachmentStorageCleanupJob>(
      "attachment-storage-cleanup",
      job => job.ExecuteAsync(JobCancellationToken.Null),
      fileStorageOptions.CleanupCronExpression);

    return app;
  }
}
