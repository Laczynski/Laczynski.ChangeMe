import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IssueAssignableUserDto, IssueDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  getDeleteIssueConfirmMessage,
  getIssuePriorityLabel,
  getIssuePrioritySeverity,
  getIssueStatusLabel,
  getIssueStatusSeverity,
  issueDeleteMenuItemDangerClasses,
  issuePriorities,
  issueStatuses
} from '@features/issues/utils/issue.utils';
import {
  GridResourceFactory,
  PrimeDataGridComponent,
  QgColumnDirective,
  QgEmptyDirective,
  type GridColumnFilter,
  type GridResource
} from '@query-grid/primeng';
import { getGridListEmptyMessage } from '@shared/data/utils/grid.utils';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';

@Component({
  selector: 'app-issues',
  imports: [
    CommonModule,
    RouterLink,
    Card,
    ButtonDirective,
    Message,
    Tag,
    Tooltip,
    Menu,
    PrimeDataGridComponent,
    QgColumnDirective,
    QgEmptyDirective
  ],
  templateUrl: './issues-list.component.html'
})
export class IssuesComponent {
  private readonly issuesService = inject(IssuesService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly gridFactory = inject(GridResourceFactory);
  private readonly destroyRef = inject(DestroyRef);

  readonly getIssueStatusLabel = getIssueStatusLabel;
  readonly getIssueStatusSeverity = getIssueStatusSeverity;
  readonly getIssuePriorityLabel = getIssuePriorityLabel;
  readonly getIssuePrioritySeverity = getIssuePrioritySeverity;
  readonly issueStatuses = issueStatuses;
  readonly issuePriorities = issuePriorities;

  readonly assignableUsers = signal<IssueAssignableUserDto[]>([]);
  readonly pendingWatchIssueIds = signal<string[]>([]);
  readonly pendingDeleteIssueIds = signal<string[]>([]);
  readonly issueActionItems = signal<MenuItem[]>([]);
  private readonly issueActionsMenu = viewChild.required<Menu>('issueActionsMenu');

  readonly grid: GridResource<IssueDto>;

  readonly assignedToFilter = computed<GridColumnFilter>(() => ({
    type: 'enum',
    options: this.assignableUsers().map((user) => ({
      label: user.displayLabel,
      value: user.id
    }))
  }));

  readonly errorMessage = computed(() => {
    const error = this.grid.error();
    return error instanceof Error ? error.message : error ? String(error) : null;
  });

  readonly emptyMessage = computed(() => getGridListEmptyMessage(this.grid.query()));

  constructor() {
    this.grid = this.gridFactory.create<IssueDto>({
      destroyRef: this.destroyRef,
      load: (query) => this.issuesService.getAllIssues(query),
      defaultSort: [{ field: 'LastActivityAt', desc: true }],
      defaultTake: 10,
      persistState: { key: 'changeme.issues-list', storage: 'session' }
    });

    this.loadAssignableUsers();
  }

  refresh(): void {
    this.grid.reload();
  }

  openIssueActionsMenu(event: Event, issue: IssueDto): void {
    this.issueActionItems.set([
      {
        label: 'Open details',
        icon: 'pi pi-eye',
        routerLink: ['/issues', issue.id]
      },
      { separator: true },
      {
        label: 'Edit issue',
        icon: 'pi pi-pencil',
        routerLink: ['/issues', issue.id, 'edit']
      },
      {
        label: 'Delete issue',
        icon: 'pi pi-trash',
        ...issueDeleteMenuItemDangerClasses,
        disabled: this.isDeletePending(issue.id),
        command: () => this.confirmDeleteIssue(issue)
      }
    ]);
    this.issueActionsMenu().toggle(event);
  }

  confirmDeleteIssue(issue: IssueDto): void {
    this.confirmationService.confirm({
      header: 'Delete issue',
      message: getDeleteIssueConfirmMessage(issue.title),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
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
      next: (watchState) => {
        this.grid.items.update((items) =>
          items.map((item) =>
            item.id === watchState.issueId
              ? {
                  ...item,
                  isWatchedByCurrentUser: watchState.isWatchedByCurrentUser,
                  watchersCount: watchState.watchersCount
                }
              : item
          )
        );
        this.setWatchPending(issue.id, false);
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
          this.grid.reload();
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
