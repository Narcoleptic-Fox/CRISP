/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import type { BaseModel } from './base-model';

export interface Todos extends BaseModel {
  title: string;
  isCompleted: boolean;
  dueDate: Date | null;
}
