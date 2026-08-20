using ChangeMe.Backend.Infrastructure.FileStorage;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;

namespace ChangeMe.Backend.Web.Configurations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class AttachmentStorageCleanupConcurrentExecutionFilterAttribute(int timeoutInSeconds)
  : JobFilterAttribute, IServerFilter
{
  private readonly DisableConcurrentExecutionAttribute _inner = new(timeoutInSeconds);

  public void OnPerforming(PerformingContext context)
  {
    if (IsAttachmentStorageCleanupJob(context.BackgroundJob.Job))
      _inner.OnPerforming(context);
  }

  public void OnPerformed(PerformedContext context)
  {
    if (IsAttachmentStorageCleanupJob(context.BackgroundJob.Job))
      _inner.OnPerformed(context);
  }

  private static bool IsAttachmentStorageCleanupJob(Job job) =>
    job.Type == typeof(AttachmentStorageCleanupJob)
    && job.Method.Name == nameof(AttachmentStorageCleanupJob.ExecuteAsync)
    && job.Method.GetParameters()[0].ParameterType == typeof(IJobCancellationToken);
}
