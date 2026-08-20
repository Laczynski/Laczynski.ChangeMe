export interface IssueDto {
  id: string;
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  createdBy: string;
  createdByName?: string | null;
  assignedToUserId?: string | null;
  assignedToUserName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  version: number;
  lastActivityAt: string;
  isWatchedByCurrentUser: boolean;
  watchersCount: number;
}

export interface IssueAssignableUserDto {
  id: string;
  displayLabel: string;
}

export interface IssueDetailsDto {
  id: string;
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  createdBy: string;
  createdByName?: string | null;
  assignedToUserId?: string | null;
  assignedToUserName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  version: number;
  lastActivityAt: string;
  isWatchedByCurrentUser: boolean;
  watchersCount: number;
  acceptanceCriteria: AcceptanceCriterionDto[];
}

export interface IssueAttachmentDto {
  id: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByUserId: string;
  uploadedByName?: string | null;
  createdAt: string;
  canDelete: boolean;
}

export interface AcceptanceCriterionDto {
  id: string;
  content: string;
  createdAt: string;
  createdBy: string;
}

export interface IssueCommentDto {
  id: string;
  content: string;
  authorUserId: string;
  authorName?: string | null;
  createdAt: string;
}

export interface AddIssueCommentRequest {
  content: string;
}

export interface IssueHistoryEntryDto {
  id: string;
  eventType: IssueHistoryEventType;
  actorUserId: string;
  actorName?: string | null;
  summary: string;
  previousValue?: string | null;
  currentValue?: string | null;
  createdAt: string;
}

export interface CreateIssueRequest {
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  assignedToUserId: string | null;
  watchAfterCreate: boolean;
  acceptanceCriteria: CreateIssueAcceptanceCriterionPayload[];
}

export interface CreateIssueAcceptanceCriterionPayload {
  content: string;
}

export interface UpdateIssueRequest {
  id: string;
  version: number;
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  assignedToUserId: string | null;
  acceptanceCriteria: UpdateIssueAcceptanceCriterionPayload[];
}

export interface UpdateIssueAcceptanceCriterionPayload {
  id?: string;
  content: string;
}

export interface IssueWatchStateDto {
  issueId: string;
  isWatchedByCurrentUser: boolean;
  watchersCount: number;
}

export enum IssueStatus {
  NEW = 'NEW',
  IN_PROGRESS = 'IN_PROGRESS',
  RESOLVED = 'RESOLVED',
  CLOSED = 'CLOSED'
}

export enum IssuePriority {
  LOW = 'LOW',
  MEDIUM = 'MEDIUM',
  HIGH = 'HIGH',
  CRITICAL = 'CRITICAL'
}

export enum IssueHistoryEventType {
  ISSUE_CREATED = 'ISSUE_CREATED',
  STATUS_CHANGED = 'STATUS_CHANGED',
  PRIORITY_CHANGED = 'PRIORITY_CHANGED',
  ASSIGNEE_CHANGED = 'ASSIGNEE_CHANGED',
  TITLE_CHANGED = 'TITLE_CHANGED',
  DESCRIPTION_CHANGED = 'DESCRIPTION_CHANGED',
  ACCEPTANCE_CRITERION_ADDED = 'ACCEPTANCE_CRITERION_ADDED',
  ACCEPTANCE_CRITERION_UPDATED = 'ACCEPTANCE_CRITERION_UPDATED',
  ACCEPTANCE_CRITERION_REMOVED = 'ACCEPTANCE_CRITERION_REMOVED',
  ATTACHMENT_ADDED = 'ATTACHMENT_ADDED',
  ATTACHMENT_REMOVED = 'ATTACHMENT_REMOVED'
}
