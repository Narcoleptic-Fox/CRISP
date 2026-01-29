/**
 * This is a TypeGen auto-generated file (manually patched).
 */

import type { ICommandWithResult } from './i-command';

// Create commands return a GUID (string) of the created entity
export interface CreateCommand extends ICommandWithResult<string> {}
