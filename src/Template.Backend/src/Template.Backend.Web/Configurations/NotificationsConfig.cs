using Hangfire;
using Microsoft.Extensions.Options;
using Template.Backend.UseCases.Issues.Services;
using Template.Backend.UseCases.Notifications.Services;
using Template.Backend.Web.Notifications;

namespace Template.Backend.Web.Configurations;

public static class NotificationsConfig
{
  public static IServiceCollection AddNotifications(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
  {
    services.AddSignalR();
    services.AddSingleton(TimeProvider.System);
    services.AddScoped<IssueNotificationService>();
    services.AddScoped<NotificationRetentionPolicy>();
    services.AddScoped<NotificationRetentionCleanupJob>();
    services.AddSingleton<INotificationRealtimePublisher, SignalRNotificationRealtimePublisher>();

    logger.LogInformation("{Project} services configured", "Notifications");
    return services;
  }

  public static WebApplication UseNotifications(this WebApplication app)
  {
    app.MapHub<NotificationHub>("/hubs/notifications");

    var retentionOptions = app.Services.GetRequiredService<IOptions<NotificationRetentionOptions>>().Value;

    var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();
    recurringJobs.AddOrUpdate<NotificationRetentionCleanupJob>(
      "notifications-retention-cleanup",
      job => job.ExecuteAsync(JobCancellationToken.Null),
      retentionOptions.CleanupCronExpression);

    return app;
  }
}
