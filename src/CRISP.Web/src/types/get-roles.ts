/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { PagedQuery } from './paged-query';
import { Roles } from './roles';
import { IQuery } from './i-query';
import { PagedResponse } from './paged-response';
import { Permissions } from './permissions';

export class GetRoles extends PagedQuery<Roles> implements IQuery<PagedResponse<Roles>> {
  names: string[] | null;
  permissions: Permissions[] | null;
}
