/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { IQuery } from './i-query';
import type { User } from './user';

export interface GetUserByEmail extends IQuery<User> {
  email: string;
}
