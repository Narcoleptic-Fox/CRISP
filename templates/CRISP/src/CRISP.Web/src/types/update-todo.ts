/**
 * This is a TypeGen auto-generated file (manually patched).
 */

import type { ModifyCommand } from './modify-command';

export interface UpdateTodo extends ModifyCommand {
  title: string;
  description: string | null;
  dueDate: Date | null;
}
