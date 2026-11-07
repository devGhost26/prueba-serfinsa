import { useCallback, useEffect, useMemo, useState } from 'react'
import { login as loginRequest } from '../api/auth'
import { SESSION_EXPIRED_EVENT } from '../api/client'
import { AuthContext } from './authContext'
import { clearSession, getSession, saveSession } from './session'

export function AuthProvider({ children }) {
  const [session, setSession] = useState(getSession)

  const login = useCallback(async (email, password) => {
    const data = await loginRequest(email, password)
    setSession(saveSession(data))
  }, [])

  const logout = useCallback(() => {
    clearSession()
    setSession(null)
  }, [])

  // Si el backend rechaza el token, se cierra la sesión.
  useEffect(() => {
    window.addEventListener(SESSION_EXPIRED_EVENT, logout)
    return () => window.removeEventListener(SESSION_EXPIRED_EVENT, logout)
  }, [logout])

  // Cuando el token vence, se cierra la sesión sin esperar a la siguiente petición.
  useEffect(() => {
    if (!session) return
    const timeout = setTimeout(logout, new Date(session.expiresAt) - new Date())
    return () => clearTimeout(timeout)
  }, [session, logout])

  const value = useMemo(
    () => ({
      user: session?.user ?? null,
      isAuthenticated: session !== null,
      isAdmin: session?.user.role === 'Admin',
      login,
      logout,
    }),
    [session, login, logout],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}
