// No authentication: there is no signed-in user and nothing is gated. This file replaces
// useCurrentUser.ts when the solution is generated with --auth none, so the pages that ask
// "may I write?" keep compiling and always hear yes.
export interface CurrentUserResult {
  status: 'authorized'
  user: null
}

export function useCurrentUser(): { data: CurrentUserResult } {
  return { data: { status: 'authorized', user: null } }
}

export function useCanWrite(): boolean {
  return true
}
