/**
 * This is a TypeGen auto-generated file (manually patched).
 * Any changes made to this file can be lost when this file is regenerated.
 */

// Base command marker interface
export interface ICommand {}

// Command with typed result
export interface ICommandWithResult<TResponse> extends ICommand {
  readonly __resultType?: TResponse;
}
