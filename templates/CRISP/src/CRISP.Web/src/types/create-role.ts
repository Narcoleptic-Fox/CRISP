/**
 * This is a TypeGen auto-generated file (manually patched).
 */

import type { CreateCommand } from './create-command';
import type { Permissions } from './permissions';

export interface CreateRole extends CreateCommand {
  name: string;
  permissions: Permissions[];
}
