/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

// Base command marker interface (no return value)
export interface ICommand {}

// Command with return value
export interface ICommandWithResult<TResponse> extends ICommand {
  readonly __resultType?: TResponse;
}
