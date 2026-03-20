'use client'

import React, { createContext, useContext, useEffect, useState } from 'react'

import { post } from '../helpers/BackendClient'

interface AuthContextValue {
  authenticated: boolean
  loading: boolean
  login: (username: string, password: string) => Promise<boolean>
  logout: () => void
}

const defaultAuthContext: AuthContextValue = {
  authenticated: false,
  loading: false,
  login: async () => false,
  logout: () => {},
}

const AuthContext = createContext<AuthContextValue>(defaultAuthContext)

export const useAuth = () => useContext(AuthContext)

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
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

  const login = async (username: string, password: string): Promise<boolean> => {
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

async function validateUserLogin(
  username: string | null,
  passwordHash: string | null,
): Promise<boolean> {
  const response = await post('user/verify', {
    username,
    passwordHash,
  })

  return response.status === 200
}
