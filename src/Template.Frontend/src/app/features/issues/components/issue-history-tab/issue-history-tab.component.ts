import { CommonModule, DatePipe } from '@angular/common';
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
import { IssueHistoryEntryDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import { getIssueHistoryEventVisual } from '@features/issues/utils/issue.utils';
import {
  createIssueTabGridQuery,
  hasMoreGridItems
} from '@shared/data/utils/grid.utils';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';
import { Timeline } from 'primeng/timeline';

@Component({
  selector: 'app-issue-history-tab',
  imports: [
    CommonModule,
    DatePipe,
    ButtonDirective,
    Message,
    ProgressSpinner,
    Tag,
    Timeline
  ],
  templateUrl: './issue-history-tab.component.html',
  host: { class: 'block' }
})
export class IssueHistoryTabComponent {
  readonly issueId = input.required<string>();

  private readonly issuesService = inject(IssuesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly getIssueHistoryEventVisual = getIssueHistoryEventVisual;

  readonly historyEntries = signal<IssueHistoryEntryDto[]>([]);
  readonly historyTotalCount = signal(0);
  readonly loadError = signal<string | null>(null);
  readonly isLoadingHistory = signal(false);
  readonly isLoadingMoreHistory = signal(false);
  readonly hasLoadedHistory = signal(false);

  readonly canShowMoreHistory = computed(() =>
    hasMoreGridItems(this.historyEntries().length, this.historyTotalCount())
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
