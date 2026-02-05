/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { Todos } from './todos';

export interface Todo extends Todos {
  description: string | null;
  createdAt: Date;
  completedAt: Date | null;
}
