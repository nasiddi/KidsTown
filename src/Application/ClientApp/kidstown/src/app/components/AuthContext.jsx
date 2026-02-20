'use client'

import React, { createContext, useContext, useEffect, useState } from 'react'
import { post } from '../helpers/BackendClient'

const defaultAuthContext = {
  authenticated: false,
  loading: false,
}
// Create the context
const AuthContext = createContext(defaultAuthContext)

// Create a custom hook for easy access to the context
export const useAuth = () => useContext(AuthContext)

// Create the provider component
export const AuthProvider = ({ children }) => {
  const [authenticated, setAuthenticated] = useState(false)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchUser = async () => {
      if (authenticated) {
        return
      }
      setLoading(true)
      try {
        const username = localStorage.getItem('username')
        const hashedPassword = localStorage.getItem('passwordHash')
        const authed = await validateUserLogin(username, hashedPassword)
        setAuthenticated(authed)
      } catch (error) {
        console.error('Failed to fetch user', error)
        setAuthenticated(false)
      } finally {
        setLoading(false)
      }
    }

    fetchUser().then()
  }, [authenticated, loading])

  const login = async (username, password) => {
    setLoading(true)
    try {
      const authed = await validateUserLogin(username, password)
      setAuthenticated(authed)
      setLoading(false)
      return authed
    } catch (error) {
      console.error('Login error:', error)
      setAuthenticated(false)
      setLoading(false)
      return false
    }
  }

  const logout = () => {
    setAuthenticated(false)
    localStorage.setItem('apiKey', '')
  }

  return (
    <AuthContext.Provider value={{ authenticated, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export async function validateUserLogin(username, passwordHash) {
  const response = await post('user/verify', {
    username,
    passwordHash,
  })

  return response.status === 200
}
