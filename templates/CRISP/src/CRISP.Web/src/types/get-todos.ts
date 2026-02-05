/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { IQuery } from './i-query';
import type { PagedResponse } from './paged-response';
import type { Todos } from './todos';
import type { PagedQuery } from './paged-query';

export interface GetTodos extends IQuery<PagedResponse<Todos>>, PagedQuery<Todos> {
  title: string | null;
  isCompleted: boolean | null;
  dueBefore: Date | null;
  dueAfter: Date | null;
}
