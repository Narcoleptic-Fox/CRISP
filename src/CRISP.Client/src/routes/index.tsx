import { createBrowserRouter, Navigate } from 'react-router-dom'
import { DashboardPage } from './dashboard'
import { NoAccessPage } from './no-access'
import { SettingsPage } from './settings'
import { ItemsPage } from './items'
import { ItemDetailPage } from './item-detail'
import { UsersPage } from './users'
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
])
