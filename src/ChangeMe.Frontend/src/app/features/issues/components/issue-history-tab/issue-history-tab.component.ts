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
import {
  ButtonComponent,
  MessageBarComponent,
  SpinnerComponent,
  TimelineComponent,
  type TimelineItem
} from '@laczynski/ui';
import { IssueHistoryEntryDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import { getIssueHistoryEventVisual } from '@features/issues/utils/issue.utils';
import {
  createIssueTabGridQuery,
  hasMoreGridItems
} from '@shared/data/utils/grid.utils';

@Component({
  selector: 'app-issue-history-tab',
  imports: [
    ButtonComponent,
    MessageBarComponent,
    SpinnerComponent,
    TimelineComponent
  ],
  templateUrl: './issue-history-tab.component.html',
  host: { class: 'block' }
})
export class IssueHistoryTabComponent {
  readonly issueId = input.required<string>();

  private readonly issuesService = inject(IssuesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly historyEntries = signal<IssueHistoryEntryDto[]>([]);
  readonly historyTotalCount = signal(0);
  readonly loadError = signal<string | null>(null);
  readonly isLoadingHistory = signal(false);
  readonly isLoadingMoreHistory = signal(false);
  readonly hasLoadedHistory = signal(false);

  readonly canShowMoreHistory = computed(() =>
    hasMoreGridItems(this.historyEntries().length, this.historyTotalCount())
  );

  readonly timelineItems = computed<TimelineItem[]>(() =>
    this.historyEntries().map((event) => {
      const visual = getIssueHistoryEventVisual(event.eventType);
      return {
        id: event.id,
        title: event.summary,
        timestamp: event.createdAt,
        meta: event.actorName || event.actorUserId,
        description: this.formatHistoryDescription(event),
        icon: visual.icon,
        variant: visual.variant
      };
    })
  );

  private lastLoadedIssueId: string | null = null;
  private historyRequestId = 0;

  constructor() {
    effect(() => {
      const issueId = this.issueId();
      if (issueId === this.lastLoadedIssueId) {
        return;
      }

      this.lastLoadedIssueId = issueId;
      this.reloadHistoryFromStart(issueId);
    });
  }

  showMoreHistory(): void {
    if (!this.canShowMoreHistory() || this.isLoadingMoreHistory()) {
      return;
    }

    this.loadHistory(this.issueId(), {
      append: true,
      skip: this.historyEntries().length
    });
  }

  private formatHistoryDescription(event: IssueHistoryEntryDto): string | undefined {
    if (
      (event.previousValue || event.currentValue) &&
      event.eventType !== 'DESCRIPTION_CHANGED'
    ) {
      return `Before: ${event.previousValue || '-'} · After: ${event.currentValue || '-'}`;
    }
    return undefined;
  }

  private reloadHistoryFromStart(issueId: string): void {
    this.loadHistory(issueId);
  }

  private loadHistory(
    issueId: string,
    options: { append?: boolean; skip?: number } = {}
  ): void {
    const append = options.append ?? false;
    const skip = options.skip ?? 0;
    const requestId = ++this.historyRequestId;

    if (append) {
      this.isLoadingMoreHistory.set(true);
    } else {
      this.isLoadingHistory.set(true);
    }

    this.loadError.set(null);

    this.issuesService
      .getIssueHistory(issueId, createIssueTabGridQuery(skip))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (requestId !== this.historyRequestId) {
            return;
          }

          if (append) {
            this.historyEntries.update((items) => [...items, ...result.items]);
          } else {
            this.historyEntries.set(result.items);
          }

          this.historyTotalCount.set(result.totalCount);
          this.isLoadingHistory.set(false);
          this.isLoadingMoreHistory.set(false);
          this.hasLoadedHistory.set(true);
        },
        error: (error: Error) => {
          if (requestId !== this.historyRequestId) {
            return;
          }

          this.loadError.set(error.message);
          this.isLoadingHistory.set(false);
          this.isLoadingMoreHistory.set(false);
        }
      });
  }
}
