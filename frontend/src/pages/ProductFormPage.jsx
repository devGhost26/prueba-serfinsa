import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { createProduct, getProduct, getProductTypes, updateProduct } from '../api/products'
import { DESCRIPTION_MAX, fromApiErrors, NAME_MAX, toProductRequest, validateProduct } from './productValidation'

const EMPTY_FORM = { name: '', description: '', price: '', stock: '', productTypeId: '' }

// Formulario para crear un producto (/productos/nuevo) o editarlo (/productos/:id/editar).
export function ProductFormPage() {
  const { id } = useParams()
  const editing = id !== undefined
  const navigate = useNavigate()

  const [form, setForm] = useState(EMPTY_FORM)
  const [productTypes, setProductTypes] = useState([])
  const [fieldErrors, setFieldErrors] = useState({})
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(true)
  const [loadFailed, setLoadFailed] = useState(false)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    const controller = new AbortController()
    Promise.all([getProductTypes(controller.signal), editing ? getProduct(id, controller.signal) : null])
      .then(([types, product]) => {
        setProductTypes(types)
        if (product) {
          setForm({
            name: product.name,
            description: product.description ?? '',
            price: product.price.toFixed(2),
            stock: String(product.stock),
            productTypeId: String(product.productTypeId),
          })
        }
      })
      .catch((err) => {
        if (err.name === 'AbortError') return
        setError(err.message)
        setLoadFailed(true)
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })
    return () => controller.abort()
  }, [editing, id])

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
    setFieldErrors((current) => ({ ...current, [name]: undefined }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)

    const errors = validateProduct(form)
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) return

    setSaving(true)
    try {
      const request = toProductRequest(form)
      const saved = editing ? await updateProduct(id, request) : await createProduct(request)
      navigate('/productos', {
        state: { notice: editing ? `Se guardaron los cambios de «${saved.name}».` : `Se creó «${saved.name}».` },
      })
    } catch (err) {
      setFieldErrors(fromApiErrors(err.errors))
      setError(err.status === 400 ? 'Revisa los campos marcados.' : err.message)
      setSaving(false)
    }
  }

  const title = editing ? 'Editar producto' : 'Nuevo producto'

  if (loading) return <p className="muted">Cargando…</p>

  if (loadFailed) {
    return (
      <section className="form-page">
        <h1>{title}</h1>
        <div className="alert alert-error" role="alert">
          {error}
        </div>
        <Link to="/productos" className="button button-secondary">
          Volver al listado
        </Link>
      </section>
    )
  }

  return (
    <section className="form-page">
      <h1>{title}</h1>
      <p className="muted">Los campos con * son obligatorios.</p>

      <form className="card" onSubmit={handleSubmit} noValidate>
        {error && (
          <div className="alert alert-error" role="alert">
            {error}
          </div>
        )}

        <Field label="Nombre *" name="name" error={fieldErrors.name}>
          <input
            id="name"
            name="name"
            value={form.name}
            onChange={handleChange}
            maxLength={NAME_MAX}
            autoFocus
            {...invalidProps('name', fieldErrors)}
          />
        </Field>

        <Field
          label="Descripción"
          name="description"
          error={fieldErrors.description}
          hint={`${form.description.length}/${DESCRIPTION_MAX}`}
        >
          <textarea
            id="description"
            name="description"
            rows={3}
            value={form.description}
            onChange={handleChange}
            maxLength={DESCRIPTION_MAX}
            {...invalidProps('description', fieldErrors)}
          />
        </Field>

        <div className="field-row">
          <Field label="Precio (USD) *" name="price" error={fieldErrors.price}>
            <input
              id="price"
              name="price"
              inputMode="decimal"
              placeholder="0.00"
              value={form.price}
              onChange={handleChange}
              {...invalidProps('price', fieldErrors)}
            />
          </Field>

          <Field label="Stock *" name="stock" error={fieldErrors.stock}>
            <input
              id="stock"
              name="stock"
              inputMode="numeric"
              placeholder="0"
              value={form.stock}
              onChange={handleChange}
              {...invalidProps('stock', fieldErrors)}
            />
          </Field>
        </div>

        <Field label="Tipo de producto *" name="productTypeId" error={fieldErrors.productTypeId}>
          <select
            id="productTypeId"
            name="productTypeId"
            value={form.productTypeId}
            onChange={handleChange}
            {...invalidProps('productTypeId', fieldErrors)}
          >
            <option value="">Elige un tipo</option>
            {productTypes.map((type) => (
              <option key={type.id} value={type.id}>
                {type.name}
              </option>
            ))}
          </select>
        </Field>

        <div className="form-actions">
          <Link to="/productos" className="button button-secondary">
            Cancelar
          </Link>
          <button type="submit" className="button button-primary" disabled={saving}>
            {saving ? 'Guardando…' : editing ? 'Guardar cambios' : 'Crear producto'}
          </button>
        </div>
      </form>
    </section>
  )
}

function Field({ label, name, error, hint, children }) {
  return (
    <div className="field">
      <label htmlFor={name}>{label}</label>
      {children}
      {(error || hint) && (
        <div className="field-footer">
          {error && (
            <span id={`${name}-error`} className="field-error">
              {error}
            </span>
          )}
          {hint && <span className="field-hint">{hint}</span>}
        </div>
      )}
    </div>
  )
}

function invalidProps(name, errors) {
  return errors[name]
    ? { 'aria-invalid': true, 'aria-describedby': `${name}-error` }
    : { 'aria-invalid': false }
}
