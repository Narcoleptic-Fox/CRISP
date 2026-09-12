import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { EventType, type AuthenticationResult } from '@azure/msal-browser'
import { MsalProvider } from '@azure/msal-react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ThemeProvider } from 'next-themes'
import { RouterProvider } from 'react-router-dom'
import { Toaster } from 'sonner'
import { msalInstance } from './auth/msalConfig'
import { router } from './routes'
import './index.css'

const queryClient = new QueryClient()

await msalInstance.initialize()

const existingAccount = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0]
if (existingAccount) {
  msalInstance.setActiveAccount(existingAccount)
}

msalInstance.addEventCallback((event) => {
  if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
    const payload = event.payload as AuthenticationResult
    msalInstance.setActiveAccount(payload.account)
  }
})

await msalInstance.handleRedirectPromise()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider attribute="class" defaultTheme="system" enableSystem>
      <MsalProvider instance={msalInstance}>
        <QueryClientProvider client={queryClient}>
          <RouterProvider router={router} />
          <Toaster position="top-right" richColors />
        </QueryClientProvider>
      </MsalProvider>
    </ThemeProvider>
  </StrictMode>,
)
