/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { BaseModel } from './base-model';
import type { Permissions } from './permissions';

export interface Roles extends BaseModel {
  name: string;
  permissions: Permissions[];
}
