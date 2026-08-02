import { signal } from '@angular/core';
import type { ConfirmMessagePart } from '@core/confirm/models/confirm-message.model';
import {
  confirmMessage,
  confirmStrong
} from '@core/confirm/utils/confirm-message.utils';
import {
  IssueHistoryEventType,
  IssuePriority,
  IssueStatus
} from '@features/issues/models/issue.model';
import type { IconName, Variant } from '@laczynski/ui';

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

export const IssueFieldErrors = {
  title: {
    required: 'Title is required',
    minlength: `Title must be at least ${IssueConstraints.TITLE_MIN_LENGTH} characters`,
    maxlength: `Title must be at most ${IssueConstraints.TITLE_MAX_LENGTH} characters`
  },
  description: {
    required: 'Description is required',
    maxlength: `Description must be at most ${IssueConstraints.DESCRIPTION_MAX_LENGTH} characters`
  },
  acceptanceCriterion: {
    required: 'Acceptance criterion is required',
    maxlength: `Acceptance criterion must be at most ${IssueAcceptanceCriteriaConstraints.CONTENT_MAX_LENGTH} characters`
  },
  comment: {
    required: 'Comment content is required',
    maxlength: `Comment must be at most ${IssueCommentConstraints.CONTENT_MAX_LENGTH} characters`
  }
} as const;

export const issueAttachmentAccept =
  IssueConstraints.ATTACHMENT_ALLOWED_EXTENSIONS.join(',');

export type IssueBadgeSeverity =
  | 'secondary'
  | 'success'
  | 'info'
  | 'warning'
  | 'danger';

export type IssueLabeledOption<T> = { value: T; label: string };

type IssueBadgeMeta<T> = IssueLabeledOption<T> & { severity: IssueBadgeSeverity };

const ISSUE_STATUS_META: IssueBadgeMeta<IssueStatus>[] = [
  { value: IssueStatus.NEW, label: 'New', severity: 'info' },
  { value: IssueStatus.IN_PROGRESS, label: 'In Progress', severity: 'warning' },
  { value: IssueStatus.RESOLVED, label: 'Resolved', severity: 'success' },
  { value: IssueStatus.CLOSED, label: 'Closed', severity: 'secondary' }
];

const ISSUE_PRIORITY_META: IssueBadgeMeta<IssuePriority>[] = [
  { value: IssuePriority.LOW, label: 'Low', severity: 'secondary' },
  { value: IssuePriority.MEDIUM, label: 'Medium', severity: 'info' },
  { value: IssuePriority.HIGH, label: 'High', severity: 'warning' },
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

export const outline = 'outline' as const;

export function getDeleteIssueConfirmMessage(title: string): ConfirmMessagePart[] {
  return confirmMessage('Delete ', confirmStrong(title), '? This cannot be undone.');
}

export function getDeleteAttachmentConfirmMessage(
  fileName: string
): ConfirmMessagePart[] {
  return confirmMessage(
    'Delete ',
    confirmStrong(fileName),
    '? This action cannot be undone.'
  );
}

export type IssueHistoryEventVisual = {
  icon: IconName;
  variant: Variant;
  tagVariant: IssueBadgeSeverity;
};

const ISSUE_HISTORY_EVENT_VISUALS: Record<
  IssueHistoryEventType,
  IssueHistoryEventVisual
> = {
  ISSUE_CREATED: {
    icon: 'add',
    variant: 'success',
    tagVariant: 'success'
  },
  STATUS_CHANGED: {
    icon: 'arrow_sync',
    variant: 'info',
    tagVariant: 'info'
  },
  PRIORITY_CHANGED: {
    icon: 'flag',
    variant: 'warning',
    tagVariant: 'warning'
  },
  ASSIGNEE_CHANGED: {
    icon: 'person',
    variant: 'info',
    tagVariant: 'info'
  },
  TITLE_CHANGED: {
    icon: 'edit',
    variant: 'secondary',
    tagVariant: 'secondary'
  },
  DESCRIPTION_CHANGED: {
    icon: 'align_left',
    variant: 'secondary',
    tagVariant: 'secondary'
  },
  ACCEPTANCE_CRITERION_ADDED: {
    icon: 'checkmark_circle',
    variant: 'success',
    tagVariant: 'success'
  },
  ACCEPTANCE_CRITERION_UPDATED: {
    icon: 'document_edit',
    variant: 'info',
    tagVariant: 'info'
  },
  ACCEPTANCE_CRITERION_REMOVED: {
    icon: 'dismiss_circle',
    variant: 'danger',
    tagVariant: 'danger'
  },
  ATTACHMENT_ADDED: {
    icon: 'attach',
    variant: 'info',
    tagVariant: 'info'
  },
  ATTACHMENT_REMOVED: {
    icon: 'delete',
    variant: 'danger',
    tagVariant: 'danger'
  }
};

const defaultIssueHistoryEventVisual: IssueHistoryEventVisual = {
  icon: 'history',
  variant: 'secondary',
  tagVariant: 'secondary'
};

export function getIssueHistoryEventVisual(
  eventType: IssueHistoryEventType
): IssueHistoryEventVisual {
  return ISSUE_HISTORY_EVENT_VISUALS[eventType] ?? defaultIssueHistoryEventVisual;
}
