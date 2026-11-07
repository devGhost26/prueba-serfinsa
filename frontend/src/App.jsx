import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { AuthProvider } from './auth/AuthProvider'
import { Layout } from './components/Layout'
import { LoginPage } from './pages/LoginPage'
import { ProductFormPage } from './pages/ProductFormPage'
import { ProductsPage } from './pages/ProductsPage'
import { ProtectedRoute } from './routes/ProtectedRoute'

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          {/* Todo lo de aquí adentro exige sesión iniciada. */}
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route path="/productos" element={<ProductsPage />} />

              {/* Crear y editar: solo el administrador. */}
              <Route element={<ProtectedRoute roles={['Admin']} />}>
                <Route path="/productos/nuevo" element={<ProductFormPage />} />
                <Route path="/productos/:id/editar" element={<ProductFormPage />} />
              </Route>
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/productos" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
