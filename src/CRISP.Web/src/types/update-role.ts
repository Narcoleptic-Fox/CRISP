/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { ModifyCommand } from './modify-command';
import { ICommand } from './i-command';
import { Permissions } from './permissions';

export class UpdateRole extends ModifyCommand implements ICommand {
  name: string = '';
  permissions: Permissions[] = [];
}
