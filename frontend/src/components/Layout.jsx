import { Outlet, useNavigate } from 'react-router'
import { useAuth } from '../auth/useAuth'

const ROLE_NAMES = { Admin: 'Administrador', User: 'Usuario' }

// Marco de las páginas privadas: barra superior con el usuario y el botón para cerrar sesión.
export function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <>
      <header className="topbar">
        <span className="topbar-title">Gestión de Productos</span>
        <div className="topbar-user">
          <span>
            {user.name} <span className="badge">{ROLE_NAMES[user.role] ?? user.role}</span>
          </span>
          <button type="button" className="button button-secondary" onClick={handleLogout}>
            Cerrar sesión
          </button>
        </div>
      </header>
      <main className="content">
        <Outlet />
      </main>
    </>
  )
}
