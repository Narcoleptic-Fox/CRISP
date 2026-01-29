/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { ModifyCommand } from './modify-command';
import type { Permissions } from './permissions';

export interface UpdateRole extends ModifyCommand {
  name: string;
  permissions: Permissions[];
}
