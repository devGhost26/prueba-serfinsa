import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { useAuth } from '../auth/useAuth'

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validate({ email, password }) {
  const errors = {}
  if (!email.trim()) errors.email = 'El correo es obligatorio.'
  else if (!EMAIL_PATTERN.test(email.trim())) errors.email = 'El correo no es válido.'
  if (!password) errors.password = 'La contraseña es obligatoria.'
  return errors
}

// El mensaje que ve el usuario según cómo respondió el backend.
function loginErrorMessage(error) {
  if (error.status === 401) return 'El correo o la contraseña no son correctos.'
  if (error.status === 429) return 'Demasiados intentos. Espera un minuto y vuelve a intentarlo.'
  return error.message
}

export function LoginPage() {
  const { login, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [form, setForm] = useState({ email: '', password: '' })
  const [fieldErrors, setFieldErrors] = useState({})
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  // Vuelve a la página que pidió antes de que lo mandaran al login.
  const destination = location.state?.from?.pathname ?? '/productos'

  if (isAuthenticated) return <Navigate to={destination} replace />

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
    setFieldErrors((current) => ({ ...current, [name]: undefined }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)

    const errors = validate(form)
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) return

    setSubmitting(true)
    try {
      await login(form.email.trim(), form.password)
      navigate(destination, { replace: true })
    } catch (err) {
      setError(loginErrorMessage(err))
      setSubmitting(false)
    }
  }

  return (
    <main className="login-page">
      <form className="card login-card" onSubmit={handleSubmit} noValidate>
        <h1>Gestión de Productos</h1>
        <p className="muted">Inicia sesión para continuar</p>

        {error && (
          <div className="alert alert-error" role="alert">
            {error}
          </div>
        )}

        <div className="field">
          <label htmlFor="email">Correo</label>
          <input
            id="email"
            name="email"
            type="email"
            autoComplete="username"
            value={form.email}
            onChange={handleChange}
            aria-invalid={Boolean(fieldErrors.email)}
            aria-describedby={fieldErrors.email ? 'email-error' : undefined}
            autoFocus
          />
          {fieldErrors.email && (
            <span id="email-error" className="field-error">
              {fieldErrors.email}
            </span>
          )}
        </div>

        <div className="field">
          <label htmlFor="password">Contraseña</label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            value={form.password}
            onChange={handleChange}
            aria-invalid={Boolean(fieldErrors.password)}
            aria-describedby={fieldErrors.password ? 'password-error' : undefined}
          />
          {fieldErrors.password && (
            <span id="password-error" className="field-error">
              {fieldErrors.password}
            </span>
          )}
        </div>

        <button type="submit" className="button button-primary" disabled={submitting}>
          {submitting ? 'Ingresando…' : 'Iniciar sesión'}
        </button>
      </form>
    </main>
  )
}
