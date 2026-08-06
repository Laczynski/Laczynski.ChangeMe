import type { Injector } from '@angular/core';
import type { GridQuery, GridResult, SortDescriptor } from '@query-grid/core';
import { createGridResource, type GridResource } from '@query-grid/ui';
import type { Observable } from 'rxjs';

export const DEFAULT_GRID_PAGE_SIZE = 10;

export const GridListMessages = {
  noItemsMatchFilters: 'No items match the filters.',
  noItemsYet: 'No items yet.'
} as const;

export function hasActiveGridQuery(query: GridQuery): boolean {
  if (query.search?.trim()) {
    return true;
  }

  return query.filter != null;
}

export function getGridListEmptyMessage(query: GridQuery): string {
  return hasActiveGridQuery(query)
    ? GridListMessages.noItemsMatchFilters
    : GridListMessages.noItemsYet;
}

export function createGridQuery(
  options: {
    skip?: number;
    take?: number;
    sort?: GridQuery['sort'];
    search?: string;
    filter?: GridQuery['filter'];
  } = {}
): GridQuery {
  return {
    skip: options.skip ?? 0,
    take: options.take ?? DEFAULT_GRID_PAGE_SIZE,
    sort: options.sort,
    search: options.search,
    filter: options.filter
  };
}

export function hasMoreGridItems(loadedCount: number, totalCount: number): boolean {
  return loadedCount < totalCount;
}

export const ISSUE_TAB_GRID_SORT = [{ field: 'CreatedAt', desc: true }];

export function createIssueTabGridQuery(skip = 0): GridQuery {
  return createGridQuery({ skip, sort: ISSUE_TAB_GRID_SORT });
}

export function createAppGridResource<T>(
  injector: Injector,
  options: {
    load: (query: GridQuery) => Observable<GridResult<T>>;
    defaultSort: SortDescriptor[];
    defaultTake?: number;
    persistKey?: string;
  }
): GridResource<T> {
  return createGridResource({
    injector,
    load: options.load,
    defaultSort: options.defaultSort,
    defaultTake: options.defaultTake ?? DEFAULT_GRID_PAGE_SIZE,
    persistState: options.persistKey
      ? { key: options.persistKey, storage: 'session' }
      : undefined
  });
}
