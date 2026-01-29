/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { PagedQuery } from './paged-query';
import { Users } from './users';
import { IQuery } from './i-query';
import { PagedResponse } from './paged-response';

export class GetUsers extends PagedQuery<Users> implements IQuery<PagedResponse<Users>> {
  emails: string[] | null;
  userNames: string[] | null;
  phoneNumbers: string[] | null;
  lockedOut: boolean | null;
}
