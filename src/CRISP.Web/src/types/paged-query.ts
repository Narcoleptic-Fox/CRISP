/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { BaseModel } from './base-model';
import { IQuery } from './i-query';
import { PagedResponse } from './paged-response';

export class PagedQuery<TResponse extends BaseModel> implements IQuery<PagedResponse<TResponse>> {
  page: number | null;
  pageSize: number | null;
  sortBy: string | null;
  sortDescending: boolean | null;
  ids: string[] | null;
  includeArchived: boolean | null;
}
