import { createContext } from 'react'

// Datos de la sesión que comparte toda la aplicación. Lo llena <AuthProvider>.
export const AuthContext = createContext(null)
