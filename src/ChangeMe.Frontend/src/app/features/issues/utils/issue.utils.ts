import { signal } from '@angular/core';
import {
  IssueHistoryEventType,
  IssuePriority,
  IssueStatus
} from '@features/issues/models/issue.model';
import { highlightDialogValue } from '@shared/ui/utils/dialog-message.utils';

export const IssueConstraints = {
  TITLE_MIN_LENGTH: 3,
  TITLE_MAX_LENGTH: 255,
  DESCRIPTION_MAX_LENGTH: 2000,
  ATTACHMENT_MAX_FILE_SIZE_BYTES: 5 * 1024 * 1024,
  ATTACHMENT_MAX_ATTACHMENTS_PER_ISSUE: 10,
  ATTACHMENT_ALLOWED_EXTENSIONS: [
    '.pdf',
    '.png',
    '.jpg',
    '.jpeg',
    '.gif',
    '.txt',
    '.csv',
    '.docx',
    '.xlsx'
  ] as const
};

export const IssueAcceptanceCriteriaConstraints = {
  CONTENT_MAX_LENGTH: 2000
};

export const IssueCommentConstraints = {
  CONTENT_MAX_LENGTH: 4000
};

export const issueAttachmentAccept =
  IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS.join(',');

export type IssueBadgeSeverity = 'secondary' | 'success' | 'info' | 'warn' | 'danger';

export type IssueLabeledOption<T> = { value: T; label: string };

type IssueBadgeMeta<T> = IssueLabeledOption<T> & { severity: IssueBadgeSeverity };

const ISSUE_STATUS_META: IssueBadgeMeta<IssueStatus>[] = [
  { value: IssueStatus.NEW, label: 'New', severity: 'info' },
  { value: IssueStatus.IN_PROGRESS, label: 'In Progress', severity: 'warn' },
  { value: IssueStatus.RESOLVED, label: 'Resolved', severity: 'success' },
  { value: IssueStatus.CLOSED, label: 'Closed', severity: 'secondary' }
];

const ISSUE_PRIORITY_META: IssueBadgeMeta<IssuePriority>[] = [
  { value: IssuePriority.LOW, label: 'Low', severity: 'secondary' },
  { value: IssuePriority.MEDIUM, label: 'Medium', severity: 'info' },
  { value: IssuePriority.HIGH, label: 'High', severity: 'warn' },
  { value: IssuePriority.CRITICAL, label: 'Critical', severity: 'danger' }
];

const issueStatusMetaByValue = new Map(
  ISSUE_STATUS_META.map((item) => [item.value, item])
);

const issuePriorityMetaByValue = new Map(
  ISSUE_PRIORITY_META.map((item) => [item.value, item])
);

export const issueStatuses = signal<IssueLabeledOption<IssueStatus>[]>(
  ISSUE_STATUS_META.map(({ value, label }) => ({ value, label }))
);

export const issuePriorities = signal<IssueLabeledOption<IssuePriority>[]>(
  ISSUE_PRIORITY_META.map(({ value, label }) => ({ value, label }))
);

export function getIssueStatusLabel(status: IssueStatus): string {
  return issueStatusMetaByValue.get(status)?.label ?? status;
}

export function getIssueStatusSeverity(status: IssueStatus): IssueBadgeSeverity {
  return issueStatusMetaByValue.get(status)?.severity ?? 'secondary';
}

export function getIssuePriorityLabel(priority: IssuePriority): string {
  return issuePriorityMetaByValue.get(priority)?.label ?? priority;
}

export function getIssuePrioritySeverity(priority: IssuePriority): IssueBadgeSeverity {
  return issuePriorityMetaByValue.get(priority)?.severity ?? 'secondary';
}

export function getDeleteIssueConfirmMessage(title: string): string {
  return `Delete ${highlightDialogValue(title)}? This cannot be undone.`;
}

export function getDeleteAttachmentConfirmMessage(fileName: string): string {
  return `Delete ${highlightDialogValue(fileName)}? This action cannot be undone.`;
}

export const issueDeleteMenuItemDangerClasses = {
  labelClass: 'text-red-600 dark:text-red-400',
  iconClass: 'text-red-600 dark:text-red-400'
} as const;

export type IssueHistoryEventVisual = {
  icon: string;
  markerClass: string;
  tagSeverity: IssueBadgeSeverity;
};

const ISSUE_HISTORY_EVENT_VISUALS: Record<
  IssueHistoryEventType,
  IssueHistoryEventVisual
> = {
  ISSUE_CREATED: {
    icon: 'pi pi-plus',
    markerClass: 'bg-green-600 text-white dark:bg-green-500',
    tagSeverity: 'success'
  },
  STATUS_CHANGED: {
    icon: 'pi pi-sync',
    markerClass: 'bg-sky-600 text-white dark:bg-sky-500',
    tagSeverity: 'info'
  },
  PRIORITY_CHANGED: {
    icon: 'pi pi-flag-fill',
    markerClass: 'bg-amber-500 text-white dark:bg-amber-400',
    tagSeverity: 'warn'
  },
  ASSIGNEE_CHANGED: {
    icon: 'pi pi-user',
    markerClass: 'bg-violet-600 text-white dark:bg-violet-500',
    tagSeverity: 'info'
  },
  TITLE_CHANGED: {
    icon: 'pi pi-pencil',
    markerClass: 'bg-zinc-500 text-white dark:bg-zinc-400',
    tagSeverity: 'secondary'
  },
  DESCRIPTION_CHANGED: {
    icon: 'pi pi-align-left',
    markerClass: 'bg-zinc-500 text-white dark:bg-zinc-400',
    tagSeverity: 'secondary'
  },
  ACCEPTANCE_CRITERION_ADDED: {
    icon: 'pi pi-check-circle',
    markerClass: 'bg-teal-600 text-white dark:bg-teal-500',
    tagSeverity: 'success'
  },
  ACCEPTANCE_CRITERION_UPDATED: {
    icon: 'pi pi-file-edit',
    markerClass: 'bg-sky-600 text-white dark:bg-sky-500',
    tagSeverity: 'info'
  },
  ACCEPTANCE_CRITERION_REMOVED: {
    icon: 'pi pi-times-circle',
    markerClass: 'bg-red-600 text-white dark:bg-red-500',
    tagSeverity: 'danger'
  },
  ATTACHMENT_ADDED: {
    icon: 'pi pi-paperclip',
    markerClass: 'bg-indigo-600 text-white dark:bg-indigo-500',
    tagSeverity: 'info'
  },
  ATTACHMENT_REMOVED: {
    icon: 'pi pi-trash',
    markerClass: 'bg-red-600 text-white dark:bg-red-500',
    tagSeverity: 'danger'
  }
};

const defaultIssueHistoryEventVisual: IssueHistoryEventVisual = {
  icon: 'pi pi-history',
  markerClass: 'bg-primary text-primary-contrast',
  tagSeverity: 'secondary'
};

export function getIssueHistoryEventVisual(
  eventType: IssueHistoryEventType
): IssueHistoryEventVisual {
  return ISSUE_HISTORY_EVENT_VISUALS[eventType] ?? defaultIssueHistoryEventVisual;
}
