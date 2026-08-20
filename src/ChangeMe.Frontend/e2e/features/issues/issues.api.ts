import {
  IssuePriority,
  IssueStatus,
  type IssueDetailsDto
} from '@features/issues/models/issue.model';
import type { E2eApiClient } from '../../shared/api/client';

export async function createIssue(
  client: E2eApiClient,
  title: string
): Promise<IssueDetailsDto> {
  return client.post<IssueDetailsDto>('/issues', {
    title,
    description: 'Created by Playwright E2E setup.',
    status: IssueStatus.NEW,
    priority: IssuePriority.MEDIUM,
    assignedToUserId: null,
    watchAfterCreate: false,
    acceptanceCriteria: []
  });
}
