/**
 * This is a TypeGen auto-generated file (manually patched).
 * Any changes made to this file can be lost when this file is regenerated.
 */

export const Permissions = {
  None: 'none',
  CanReadUser: 'canReadUser',
  CanUpdateUser: 'canUpdateUser',
  CanDeleteUser: 'canDeleteUser',
  CanCreateRole: 'canCreateRole',
  CanReadRole: 'canReadRole',
  CanUpdateRole: 'canUpdateRole',
  CanDeleteRole: 'canDeleteRole',
  AccessAll: 'accessAll',
} as const;

export type Permissions = typeof Permissions[keyof typeof Permissions];
