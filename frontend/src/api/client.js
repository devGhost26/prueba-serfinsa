import { clearSession, getToken } from '../auth/session'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5142/api'

// Evento que avisa a la aplicación que el backend rechazó el token (vencido o inválido).
export const SESSION_EXPIRED_EVENT = 'session-expired'

// Error de la API con los datos del ProblemDetails que devuelve el backend.
export class ApiError extends Error {
  constructor(status, problem) {
    super(problem?.detail ?? problem?.title ?? 'Ocurrió un error inesperado.')
    this.status = status
    this.title = problem?.title
    // Errores por campo: { Name: ['El nombre es obligatorio.'] }
    this.errors = problem?.errors ?? {}
  }
}

// Hace una petición a la API. Agrega el token si hay sesión y devuelve el JSON de la respuesta.
// Con `signal` la petición se puede cancelar, por ejemplo si se sale de la página antes de que responda.
export async function apiFetch(path, { method = 'GET', body, signal } = {}) {
  const token = getToken()
  const headers = { Accept: 'application/json' }
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  if (token) headers.Authorization = `Bearer ${token}`

  let response
  try {
    response = await fetch(`${API_URL}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    })
  } catch (error) {
    if (error.name === 'AbortError') throw error
    throw new ApiError(0, { detail: 'No se pudo conectar con el servidor. Revisa tu conexión.' })
  }

  if (response.status === 401 && token) {
    clearSession()
    window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT))
  }

  if (response.status === 204) return null

  const data = await response.json().catch(() => null)
  if (!response.ok) throw new ApiError(response.status, data)
  return data
}
