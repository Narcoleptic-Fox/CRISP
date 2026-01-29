/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { BaseModel } from './base-model';
import { Permissions } from './permissions';

export class Roles extends BaseModel {
  name: string = '';
  permissions: Permissions[] = [];
}
