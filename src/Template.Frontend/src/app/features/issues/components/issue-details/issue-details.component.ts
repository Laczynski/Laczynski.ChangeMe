import { DatePipe } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IssueAttachmentsTabComponent } from '@features/issues/components/issue-attachments-tab/issue-attachments-tab.component';
import { IssueCommentsTabComponent } from '@features/issues/components/issue-comments-tab/issue-comments-tab.component';
import { IssueHistoryTabComponent } from '@features/issues/components/issue-history-tab/issue-history-tab.component';
import { IssueDetailsDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  getDeleteIssueConfirmMessage,
  getIssuePriorityLabel,
  getIssuePrioritySeverity,
  getIssueStatusLabel,
  getIssueStatusSeverity
} from '@features/issues/utils/issue.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { ConfirmationService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primeng/tabs';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';

type IssueDetailsTab = 'comments' | 'attachments' | 'history';

function resolveIssueDetailsTab(
  tab: string | number | null | undefined
): IssueDetailsTab {
  if (tab === 'history' || tab === 'attachments') {
    return tab;
  }

  return 'comments';
}

@Component({
  selector: 'app-issue-details',
  imports: [
    DatePipe,
    RouterLink,
    Card,
    ButtonDirective,
    Message,
    Tag,
    Panel,
    ProgressSpinner,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    Tooltip,
    BackButtonComponent,
    IssueCommentsTabComponent,
    IssueAttachmentsTabComponent,
    IssueHistoryTabComponent
  ],
  templateUrl: './issue-details.component.html'
})
export class IssueDetailsComponent {
  readonly id = input<string>();

  private readonly issuesService = inject(IssuesService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly getIssueStatusLabel = getIssueStatusLabel;
  readonly getIssueStatusSeverity = getIssueStatusSeverity;
  readonly getIssuePriorityLabel = getIssuePriorityLabel;
  readonly getIssuePrioritySeverity = getIssuePrioritySeverity;

  readonly issue = signal<IssueDetailsDto | null>(null);
  readonly pageTitle = computed(() => this.issue()?.title ?? 'Issue Details');
  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly isTogglingWatch = signal(false);
  readonly isDeleting = signal(false);
  readonly activeTab = signal<IssueDetailsTab>('comments');

  private lastLoadedIssueId: string | null = null;

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        this.activeTab.set(resolveIssueDetailsTab(params.get('tab')));
      });

    effect(() => {
      const id = this.id();
      if (!id) {
        this.isLoading.set(false);
        return;
      }

      if (id !== this.lastLoadedIssueId) {
        this.lastLoadedIssueId = id;
        this.loadIssue(id);
      }
    });
  }

  onTabChange(tab: string | number | undefined): void {
    const value = resolveIssueDetailsTab(tab);
    if (this.activeTab() === value) {
      return;
    }

    this.activeTab.set(value);

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        tab: value === 'comments' ? null : value
      },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  getWatchTooltip(issue: IssueDetailsDto): string {
    const watchers = this.formatWatchersCount(issue.watchersCount);
    return issue.isWatchedByCurrentUser
      ? `Unwatch this issue (${watchers})`
      : `Watch this issue (${watchers})`;
  }

  confirmDeleteIssue(): void {
    const issue = this.issue();
    if (!issue || this.isDeleting()) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Delete issue',
      message: getDeleteIssueConfirmMessage(issue.title),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.deleteIssue(issue.id)
    });
  }

  toggleWatch(): void {
    const issue = this.issue();
    if (!issue || this.isTogglingWatch()) {
      return;
    }

    this.isTogglingWatch.set(true);

    const request = issue.isWatchedByCurrentUser
      ? this.issuesService.unwatchIssue(issue.id)
      : this.issuesService.watchIssue(issue.id);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (watchState) => {
        this.issue.update((currentIssue) =>
          currentIssue
            ? {
                ...currentIssue,
                isWatchedByCurrentUser: watchState.isWatchedByCurrentUser,
                watchersCount: watchState.watchersCount
              }
            : currentIssue
        );
        this.isTogglingWatch.set(false);
      },
      error: (error: Error) => {
        this.loadError.set(error.message);
        this.isTogglingWatch.set(false);
      }
    });
  }

  refresh(): void {
    const issueId = this.id();
    if (!issueId) {
      return;
    }
    this.loadIssue(issueId);
  }

  private deleteIssue(issueId: string): void {
    this.isDeleting.set(true);
    this.loadError.set(null);

    this.issuesService
      .deleteIssue(issueId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success('Issue deleted');
          void this.router.navigate(['/issues']);
        },
        error: (error: Error) => {
          this.toastService.showApiError(error, 'Could not delete issue');
          this.isDeleting.set(false);
        }
      });
  }

  private loadIssue(issueId: string): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.issuesService
      .getIssue(issueId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (issue) => {
          this.issue.set(issue);
          this.isLoading.set(false);
          this.loadError.set(null);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  private formatWatchersCount(count: number): string {
    return count === 1 ? '1 watcher' : `${count} watchers`;
  }
}
