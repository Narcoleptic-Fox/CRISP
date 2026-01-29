/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { IQuery } from './i-query';
import type { PagedResponse } from './paged-response';
import type { Roles } from './roles';
import type { PagedQuery } from './paged-query';
import type { Permissions } from './permissions';

export interface GetRoles extends IQuery<PagedResponse<Roles>>, PagedQuery<Roles> {
  names: string[] | null;
  permissions: Permissions[] | null;
}
