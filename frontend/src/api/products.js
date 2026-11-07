import { apiFetch } from './client'

export function getProducts(signal) {
  return apiFetch('/products', { signal })
}

export function getProduct(id, signal) {
  return apiFetch(`/products/${id}`, { signal })
}

export function createProduct(product) {
  return apiFetch('/products', { method: 'POST', body: product })
}

export function updateProduct(id, product) {
  return apiFetch(`/products/${id}`, { method: 'PUT', body: product })
}

export function deleteProduct(id) {
  return apiFetch(`/products/${id}`, { method: 'DELETE' })
}

export function getProductTypes(signal) {
  return apiFetch('/product-types', { signal })
}
