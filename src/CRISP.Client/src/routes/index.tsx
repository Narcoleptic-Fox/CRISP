import { createBrowserRouter, Navigate } from 'react-router-dom'
import { DashboardPage } from './dashboard'
//#if (IncludeAuth)
import { NoAccessPage } from './no-access'
import { SettingsPage } from './settings'
//#endif
//#if (IncludeSample)
import { ItemsPage } from './items'
import { ItemDetailPage } from './item-detail'
//#endif
//#if (IncludeAuth)
import { UsersPage } from './users'
//#endif
import { ProtectedRoute } from './protected-route'

export const router = createBrowserRouter([
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <DashboardPage />
      </ProtectedRoute>
    ),
  },
  //#if (IncludeSample)
  {
    path: '/items',
    element: (
      <ProtectedRoute>
        <ItemsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: '/items/:id',
    element: (
      <ProtectedRoute>
        <ItemDetailPage />
      </ProtectedRoute>
    ),
  },
  //#endif
  //#if (IncludeAuth)
  {
    path: '/users',
    element: (
      <ProtectedRoute>
        <UsersPage />
      </ProtectedRoute>
    ),
  },
  {
    path: '/settings',
    element: (
      <ProtectedRoute>
        <SettingsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: '/signin-oidc',
    element: <Navigate to="/" replace />,
  },
  {
    path: '/no-access',
    element: <NoAccessPage />,
  },
  //#endif
])
