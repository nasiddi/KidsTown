import type React from 'react'
import { useEffect } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'

import { useAuth } from './AuthContext'

function withAuth<P extends object>(WrappedComponent: React.ComponentType<P>) {
  return function AuthComponent(props: P) {
    const { authenticated, loading } = useAuth()
    const location = useLocation()
    const navigate = useNavigate()

    useEffect(() => {
      if (!loading && !authenticated) {
        const redirect = encodeURIComponent(location.pathname + location.search)
        navigate(`/login?redirect=${redirect}`, { replace: true })
      }
    }, [authenticated, loading, location, navigate])

    if (loading) {
      return <div>Loading...</div>
    }

    if (!authenticated) {
      return null
    }

    return <WrappedComponent {...props} />
  }
}

export default withAuth
