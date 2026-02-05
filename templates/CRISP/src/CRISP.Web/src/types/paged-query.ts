/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { BaseModel } from './base-model';
import type { IQuery } from './i-query';
import type { PagedResponse } from './paged-response';

export interface PagedQuery<TResponse extends BaseModel> extends IQuery<PagedResponse<TResponse>> {
  page: number | null;
  pageSize: number | null;
  sortBy: string | null;
  sortDescending: boolean | null;
  ids: string[] | null;
  includeArchived: boolean | null;
}
