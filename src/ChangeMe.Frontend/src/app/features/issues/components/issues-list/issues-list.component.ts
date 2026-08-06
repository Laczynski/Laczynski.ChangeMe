import { DatePipe } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  inject,
  Injector,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { ConfirmService } from '@core/confirm/services/confirm.service';
import { ToastService } from '@core/toast/services/toast.service';
import { IssueAssignableUserDto, IssueDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  getDeleteIssueConfirmMessage,
  getIssuePriorityLabel,
  getIssuePrioritySeverity,
  getIssueStatusLabel,
  getIssueStatusSeverity,
  issuePriorities,
  issueStatuses
} from '@features/issues/utils/issue.utils';
import {
  ButtonComponent,
  MenuComponent,
  MessageBarComponent,
  TagComponent,
  TooltipDirective,
  type MenuItem
} from '@laczynski/ui';
import { formatGridError } from '@query-grid/core';
import {
  QgColumnDirective,
  QgEmptyDirective,
  UiDataGridComponent
} from '@query-grid/ui';
import {
  createAppGridResource,
  getGridListEmptyMessage
} from '@shared/data/utils/grid.utils';

@Component({
  selector: 'app-issues',
  imports: [
    DatePipe,
    RouterLink,
    ButtonComponent,
    MessageBarComponent,
    TagComponent,
    TooltipDirective,
    MenuComponent,
    UiDataGridComponent,
    QgColumnDirective,
    QgEmptyDirective
  ],
  templateUrl: './issues-list.component.html'
})
export class IssuesComponent {
  private readonly issuesService = inject(IssuesService);
  private readonly confirmService = inject(ConfirmService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly injector = inject(Injector);

  readonly getIssueStatusLabel = getIssueStatusLabel;
  readonly getIssueStatusSeverity = getIssueStatusSeverity;
  readonly getIssuePriorityLabel = getIssuePriorityLabel;
  readonly getIssuePrioritySeverity = getIssuePrioritySeverity;
  readonly issueStatuses = issueStatuses;
  readonly issuePriorities = issuePriorities;
  readonly getGridListEmptyMessage = getGridListEmptyMessage;

  readonly assignableUsers = signal<IssueAssignableUserDto[]>([]);
  readonly pendingWatchIssueIds = signal<string[]>([]);
  readonly pendingDeleteIssueIds = signal<string[]>([]);

  protected readonly rowType!: IssueDto;

  readonly grid = createAppGridResource(this.injector, {
    load: (query) => this.issuesService.getAllIssues(query),
    defaultSort: [{ field: 'lastActivityAt', desc: true }],
    defaultTake: 10,
    persistKey: 'changeme.issues-list'
  });

  readonly assignableUserFilterOptions = computed(() =>
    this.assignableUsers().map((user) => ({
      label: user.displayLabel,
      value: user.id
    }))
  );

  readonly errorMessage = computed(() => formatGridError(this.grid.error()));

  constructor() {
    this.loadAssignableUsers();
  }

  refresh(): void {
    this.grid.reload();
  }

  getIssueMenuItems(issue: IssueDto): MenuItem[] {
    return [
      { id: 'open', label: 'Open details', icon: 'eye' },
      { id: 'edit', label: 'Edit issue', icon: 'edit' },
      {
        id: 'delete',
        label: 'Delete issue',
        icon: 'delete',
        variant: 'danger',
        disabled: this.isDeletePending(issue.id)
      }
    ];
  }

  onIssueMenuAction(item: MenuItem, issue: IssueDto): void {
    switch (item.id) {
      case 'open':
        void this.router.navigate(['/issues', issue.id]);
        break;
      case 'edit':
        void this.router.navigate(['/issues', issue.id, 'edit']);
        break;
      case 'delete':
        this.confirmDeleteIssue(issue);
        break;
    }
  }

  confirmDeleteIssue(issue: IssueDto): void {
    this.confirmService.confirm({
      header: 'Delete issue',
      message: getDeleteIssueConfirmMessage(issue.title),
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptVariant: 'danger',
      accept: () => this.deleteIssue(issue)
    });
  }

  isDeletePending(issueId: string): boolean {
    return this.pendingDeleteIssueIds().includes(issueId);
  }

  getWatchTooltip(issue: IssueDto): string {
    const watchers = this.formatWatchersCount(issue.watchersCount);
    return issue.isWatchedByCurrentUser
      ? `Unwatch this issue (${watchers})`
      : `Watch this issue (${watchers})`;
  }

  toggleWatch(issue: IssueDto): void {
    if (this.isWatchPending(issue.id)) {
      return;
    }

    this.setWatchPending(issue.id, true);

    const request = issue.isWatchedByCurrentUser
      ? this.issuesService.unwatchIssue(issue.id)
      : this.issuesService.watchIssue(issue.id);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.setWatchPending(issue.id, false);
        this.refresh();
      },
      error: () => {
        this.setWatchPending(issue.id, false);
      }
    });
  }

  isWatchPending(issueId: string): boolean {
    return this.pendingWatchIssueIds().includes(issueId);
  }

  private deleteIssue(issue: IssueDto): void {
    if (this.isDeletePending(issue.id)) {
      return;
    }

    this.setDeletePending(issue.id, true);

    this.issuesService
      .deleteIssue(issue.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.refresh();
          this.setDeletePending(issue.id, false);
          this.toastService.success('Issue deleted', issue.title);
        },
        error: (error: Error) => {
          this.toastService.showApiError(error, 'Could not delete issue');
          this.setDeletePending(issue.id, false);
        }
      });
  }

  private loadAssignableUsers(): void {
    this.issuesService
      .getAssignableUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => this.assignableUsers.set(users)
      });
  }

  private setWatchPending(issueId: string, isPending: boolean): void {
    this.pendingWatchIssueIds.update((issueIds) =>
      isPending ? [...issueIds, issueId] : issueIds.filter((id) => id !== issueId)
    );
  }

  private setDeletePending(issueId: string, isPending: boolean): void {
    this.pendingDeleteIssueIds.update((issueIds) =>
      isPending ? [...issueIds, issueId] : issueIds.filter((id) => id !== issueId)
    );
  }

  private formatWatchersCount(count: number): string {
    return count === 1 ? '1 watcher' : `${count} watchers`;
  }
}
