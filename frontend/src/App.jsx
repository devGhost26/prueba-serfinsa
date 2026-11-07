import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { AuthProvider } from './auth/AuthProvider'
import { Layout } from './components/Layout'
import { LoginPage } from './pages/LoginPage'
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
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/productos" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
