import type { GridQuery } from '@laczynski/datagrid';

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
