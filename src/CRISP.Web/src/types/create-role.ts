/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { CreateCommand } from './create-command';
import type { Permissions } from './permissions';

export interface CreateRole extends CreateCommand {
  name: string;
  permissions: Permissions[];
}
