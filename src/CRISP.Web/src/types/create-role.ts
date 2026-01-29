/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { CreateCommand } from './create-command';
import { ICommand } from './i-command';
import { Permissions } from './permissions';

export class CreateRole extends CreateCommand implements ICommand<string> {
  name: string = '';
  permissions: Permissions[] = [];
}
