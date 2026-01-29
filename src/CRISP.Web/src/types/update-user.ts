/**
 * This is a TypeGen auto-generated file (manually patched).
 */

import type { ModifyCommand } from './modify-command';

export interface UpdateUser extends ModifyCommand {
  userName: string;
  email: string;
  phoneNumber: string | null;
}
