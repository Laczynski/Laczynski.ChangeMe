import { Component, computed, input, output } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideChevronLeft,
  lucideChevronRight,
  lucideChevronsLeft,
  lucideChevronsRight
} from '@ng-icons/lucide';
import {
  DEFAULT_GRID_PAGE_SIZE,
  type GridPageChangeEvent
} from '@shared/data/utils/grid.utils';
import { HlmButtonImports } from '@spartan/ui/button';

@Component({
  selector: 'app-grid-paginator',
  imports: [...HlmButtonImports, NgIcon],
  providers: [
    provideIcons({
      lucideChevronLeft,
      lucideChevronRight,
      lucideChevronsLeft,
      lucideChevronsRight
    })
  ],
  template: `
    <nav
      class="flex flex-wrap items-center justify-between gap-3 pt-4"
      aria-label="Pagination"
    >
      <p class="text-muted-foreground m-0 text-sm">
        @if (totalCount() === 0) {
          No results
        } @else {
          Showing {{ rangeStart() }}–{{ rangeEnd() }} of {{ totalCount() }}
        }
      </p>

      <div class="flex items-center gap-1">
        <button
          type="button"
          hlmBtn
          variant="outline"
          size="icon-sm"
          aria-label="First page"
          [disabled]="!canGoPrevious()"
          (click)="goToPage(0)"
        >
          <ng-icon name="lucideChevronsLeft" aria-hidden="true" />
        </button>
        <button
          type="button"
          hlmBtn
          variant="outline"
          size="icon-sm"
          aria-label="Previous page"
          [disabled]="!canGoPrevious()"
          (click)="goToPage(skip() - take())"
        >
          <ng-icon name="lucideChevronLeft" aria-hidden="true" />
        </button>
        <button
          type="button"
          hlmBtn
          variant="outline"
          size="icon-sm"
          aria-label="Next page"
          [disabled]="!canGoNext()"
          (click)="goToPage(skip() + take())"
        >
          <ng-icon name="lucideChevronRight" aria-hidden="true" />
        </button>
        <button
          type="button"
          hlmBtn
          variant="outline"
          size="icon-sm"
          aria-label="Last page"
          [disabled]="!canGoNext()"
          (click)="goToPage(lastPageSkip())"
        >
          <ng-icon name="lucideChevronsRight" aria-hidden="true" />
        </button>
      </div>
    </nav>
  `
})
export class GridPaginatorComponent {
  readonly totalCount = input.required<number>();
  readonly skip = input(0);
  readonly take = input(DEFAULT_GRID_PAGE_SIZE);

  readonly pageChange = output<GridPageChangeEvent>();

  readonly canGoPrevious = computed(() => this.skip() > 0);
  readonly canGoNext = computed(() => this.skip() + this.take() < this.totalCount());
  readonly rangeStart = computed(() => (this.totalCount() === 0 ? 0 : this.skip() + 1));
  readonly rangeEnd = computed(() =>
    Math.min(this.skip() + this.take(), this.totalCount())
  );
  readonly lastPageSkip = computed(() => {
    const pageSize = this.take();
    const total = this.totalCount();
    if (total === 0) {
      return 0;
    }

    return Math.floor((total - 1) / pageSize) * pageSize;
  });

  goToPage(skip: number): void {
    const normalizedSkip = Math.max(0, Math.min(skip, this.lastPageSkip()));
    this.pageChange.emit({ skip: normalizedSkip, take: this.take() });
  }
}
