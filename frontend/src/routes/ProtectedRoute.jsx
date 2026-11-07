import { Navigate, Outlet, useLocation } from 'react-router'
import { useAuth } from '../auth/useAuth'

// Deja pasar solo a quien tiene sesión y, si se indican roles, a quien tiene uno de ellos.
// Sin sesión manda al login y recuerda a dónde quería ir; sin el rol, vuelve al listado.
export function ProtectedRoute({ roles }) {
  const { isAuthenticated, user } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (roles && !roles.includes(user.role)) {
    return <Navigate to="/productos" replace />
  }

  return <Outlet />
}
