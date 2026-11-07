// La sesión se guarda en localStorage: el token, cuándo vence y los datos del usuario.
const STORAGE_KEY = 'session'

export function getSession() {
  try {
    const session = JSON.parse(localStorage.getItem(STORAGE_KEY))
    if (!session?.token || !session.expiresAt) return null

    // Un token vencido no sirve: se borra para no enviarlo.
    if (new Date(session.expiresAt) <= new Date()) {
      clearSession()
      return null
    }
    return session
  } catch {
    return null
  }
}

export function getToken() {
  return getSession()?.token ?? null
}

export function saveSession({ token, expiresAt, name, email, role }) {
  const session = { token, expiresAt, user: { name, email, role } }
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
  return session
}

export function clearSession() {
  try {
    localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Si el navegador no deja usar localStorage, no hay nada que borrar.
  }
}
