const currency = new Intl.NumberFormat('es-US', { style: 'currency', currency: 'USD' })

export function formatPrice(value) {
  return currency.format(value)
}
