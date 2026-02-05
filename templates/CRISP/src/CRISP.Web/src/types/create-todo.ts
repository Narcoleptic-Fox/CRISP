/**
 * This is a TypeGen auto-generated file (manually patched).
 */

import type { CreateCommand } from './create-command';

export interface CreateTodo extends CreateCommand {
  title: string;
  description: string | null;
  dueDate: Date | null;
}
