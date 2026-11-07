export const NAME_MAX = 100
export const DESCRIPTION_MAX = 500

// Precio: número positivo con hasta 2 decimales. Stock: entero desde 0.
const PRICE_PATTERN = /^\d+(\.\d{1,2})?$/
const STOCK_PATTERN = /^\d+$/

// Las mismas reglas que valida el backend, para avisar antes de enviar.
export function validateProduct(form) {
  const errors = {}
  const name = form.name.trim()
  const price = form.price.trim()
  const stock = form.stock.trim()

  if (!name) errors.name = 'El nombre es obligatorio.'
  else if (name.length > NAME_MAX) errors.name = `El nombre no puede pasar de ${NAME_MAX} caracteres.`

  if (form.description.trim().length > DESCRIPTION_MAX)
    errors.description = `La descripción no puede pasar de ${DESCRIPTION_MAX} caracteres.`

  if (!price) errors.price = 'El precio es obligatorio.'
  else if (!PRICE_PATTERN.test(price)) errors.price = 'Escribe un precio válido, con hasta 2 decimales (por ejemplo 12.50).'
  else if (Number(price) <= 0) errors.price = 'El precio debe ser mayor que 0.'

  if (!stock) errors.stock = 'El stock es obligatorio.'
  else if (!STOCK_PATTERN.test(stock)) errors.stock = 'El stock debe ser un número entero, sin negativos.'

  if (!form.productTypeId) errors.productTypeId = 'Elige un tipo de producto.'

  return errors
}

// Convierte el formulario en lo que espera la API.
export function toProductRequest(form) {
  return {
    name: form.name.trim(),
    description: form.description.trim() || null,
    price: Number(form.price),
    stock: Number(form.stock),
    productTypeId: Number(form.productTypeId),
  }
}

// Los errores por campo que devuelve el backend vienen como { Name: [...] }; el formulario usa { name: '...' }.
export function fromApiErrors(errors) {
  return Object.fromEntries(
    Object.entries(errors).map(([field, messages]) => [field.charAt(0).toLowerCase() + field.slice(1), messages[0]]),
  )
}
