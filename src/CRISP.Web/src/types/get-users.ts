/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { IQuery } from './i-query';
import type { PagedResponse } from './paged-response';
import type { Users } from './users';
import type { PagedQuery } from './paged-query';

export interface GetUsers extends IQuery<PagedResponse<Users>>, PagedQuery<Users> {
  emails: string[] | null;
  userNames: string[] | null;
  phoneNumbers: string[] | null;
  lockedOut: boolean | null;
}
