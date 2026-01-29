/**
 * This is a TypeGen auto-generated file (manually patched).
 */

import type { ModifyCommand } from './modify-command';
import type { Permissions } from './permissions';

export interface UpdateRole extends ModifyCommand {
  name: string;
  permissions: Permissions[];
}
