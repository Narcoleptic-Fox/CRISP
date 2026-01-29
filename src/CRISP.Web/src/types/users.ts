/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { BaseModel } from './base-model';

export class Users extends BaseModel {
  userName: string = '';
  email: string = '';
  phoneNumber: string | null;
  lockoutEnd: Date | null;
  lockOutEnabled: boolean;
}
