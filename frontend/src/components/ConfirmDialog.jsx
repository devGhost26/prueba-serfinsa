import { useEffect, useRef } from 'react'

// Ventana de confirmación para acciones que no se pueden deshacer.
// Se muestra mientras `open` sea verdadero; Escape o «Cancelar» la cierran.
export function ConfirmDialog({ open, title, message, confirmLabel, busy, onConfirm, onCancel }) {
  const dialogRef = useRef(null)

  useEffect(() => {
    const dialog = dialogRef.current
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  function handleCancel(event) {
    event.preventDefault()
    if (!busy) onCancel()
  }

  return (
    <dialog ref={dialogRef} className="dialog" onCancel={handleCancel} aria-labelledby="confirm-title">
      <h2 id="confirm-title">{title}</h2>
      <p>{message}</p>
      <div className="dialog-actions">
        <button type="button" className="button button-secondary" onClick={onCancel} disabled={busy}>
          Cancelar
        </button>
        <button type="button" className="button button-danger" onClick={onConfirm} disabled={busy}>
          {busy ? 'Eliminando…' : confirmLabel}
        </button>
      </div>
    </dialog>
  )
}
