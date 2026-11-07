import { useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { deleteProduct, getProducts } from '../api/products'
import { useAuth } from '../auth/useAuth'
import { ConfirmDialog } from '../components/ConfirmDialog'
import { formatPrice } from '../utils/format'

const NOTICE_DURATION_MS = 4000

// Listado de productos. Todos los usuarios con sesión lo ven;
// los botones para crear, editar y eliminar aparecen solo para el administrador.
export function ProductsPage() {
  const { isAdmin } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()

  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [search, setSearch] = useState('')
  const [notice, setNotice] = useState(location.state?.notice ?? null)
  const [toDelete, setToDelete] = useState(null)
  const [deleting, setDeleting] = useState(false)

  // El aviso que deja el formulario al guardar se muestra una vez: se quita del historial
  // para que no vuelva a aparecer al recargar.
  useEffect(() => {
    if (location.state?.notice) navigate(location.pathname, { replace: true, state: null })
  }, [location, navigate])

  // El aviso se oculta solo a los pocos segundos.
  useEffect(() => {
    if (!notice) return
    const timeout = setTimeout(() => setNotice(null), NOTICE_DURATION_MS)
    return () => clearTimeout(timeout)
  }, [notice])

  useEffect(() => {
    const controller = new AbortController()
    getProducts(controller.signal)
      .then(setProducts)
      .catch((err) => {
        if (err.name !== 'AbortError') setError(err.message)
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })
    return () => controller.abort()
  }, [])

  const visibleProducts = useMemo(() => {
    const term = search.trim().toLowerCase()
    if (!term) return products
    return products.filter(
      (p) => p.name.toLowerCase().includes(term) || p.productTypeName.toLowerCase().includes(term),
    )
  }, [products, search])

  async function handleDelete() {
    setDeleting(true)
    try {
      await deleteProduct(toDelete.id)
      setProducts((current) => current.filter((p) => p.id !== toDelete.id))
      setNotice(`Se eliminó «${toDelete.name}».`)
      setError(null)
    } catch (err) {
      // Si otro usuario ya lo había eliminado, igual se quita de la lista.
      if (err.status === 404) setProducts((current) => current.filter((p) => p.id !== toDelete.id))
      setError(err.message)
      setNotice(null)
    } finally {
      setDeleting(false)
      setToDelete(null)
    }
  }

  return (
    <section>
      <div className="page-header">
        <div>
          <h1>Productos</h1>
          {!loading && !error && (
            <p className="muted">
              {products.length === 1 ? '1 producto' : `${products.length} productos`}
            </p>
          )}
        </div>
        {isAdmin && (
          <Link to="/productos/nuevo" className="button button-primary">
            Nuevo producto
          </Link>
        )}
      </div>

      {notice && (
        <div className="alert alert-success alert-dismissible" role="status">
          <span>{notice}</span>
          <button type="button" className="alert-close" aria-label="Cerrar aviso" onClick={() => setNotice(null)}>
            ×
          </button>
        </div>
      )}
      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}

      {loading ? (
        <p className="muted">Cargando productos…</p>
      ) : (
        products.length > 0 && (
          <>
            <div className="toolbar">
              <input
                type="search"
                className="search"
                placeholder="Buscar por nombre o tipo"
                aria-label="Buscar productos"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
              />
            </div>

            <div className="table-wrapper card">
              <table className="table">
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th>Tipo</th>
                    <th className="number">Precio</th>
                    <th className="number">Stock</th>
                    {isAdmin && <th className="actions-column">Acciones</th>}
                  </tr>
                </thead>
                <tbody>
                  {visibleProducts.map((product) => (
                    <tr key={product.id}>
                      <td>
                        <div className="product-name">{product.name}</div>
                        {product.description && <div className="product-description">{product.description}</div>}
                      </td>
                      <td>{product.productTypeName}</td>
                      <td className="number">{formatPrice(product.price)}</td>
                      <td className="number">
                        {product.stock === 0 ? <span className="badge badge-warning">Agotado</span> : product.stock}
                      </td>
                      {isAdmin && (
                        <td>
                          <div className="actions">
                            <Link
                              to={`/productos/${product.id}/editar`}
                              className="button button-secondary button-small"
                              aria-label={`Editar ${product.name}`}
                            >
                              Editar
                            </Link>
                            <button
                              type="button"
                              className="button button-danger-outline button-small"
                              aria-label={`Eliminar ${product.name}`}
                              onClick={() => setToDelete(product)}
                            >
                              Eliminar
                            </button>
                          </div>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
              {visibleProducts.length === 0 && <p className="empty">Ningún producto coincide con «{search}».</p>}
            </div>
          </>
        )
      )}

      {!loading && !error && products.length === 0 && (
        <div className="card empty">
          <p>Todavía no hay productos.</p>
          {isAdmin && (
            <Link to="/productos/nuevo" className="button button-primary">
              Crear el primero
            </Link>
          )}
        </div>
      )}

      <ConfirmDialog
        open={toDelete !== null}
        title="Eliminar producto"
        message={toDelete && `¿Eliminar «${toDelete.name}»? Esta acción no se puede deshacer.`}
        confirmLabel="Eliminar"
        busy={deleting}
        onConfirm={handleDelete}
        onCancel={() => setToDelete(null)}
      />
    </section>
  )
}
