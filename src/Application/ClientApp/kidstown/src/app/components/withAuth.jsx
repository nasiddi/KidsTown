import React, { useEffect } from 'react'
import { useAuth } from './AuthContext'
import { useLocation, useNavigate } from 'react-router-dom'

function withAuth(WrappedComponent) {
  return function AuthComponent(props) {
    const { authenticated, loading } = useAuth()
    const location = useLocation()
    const navigate = useNavigate()

    useEffect(() => {
      if (!loading && !authenticated) {
        // Redirect to login and preserve current path
        const redirect = encodeURIComponent(location.pathname + location.search)
        navigate(`/login?redirect=${redirect}`, { replace: true })
      }
    }, [authenticated, loading, location, navigate])

    if (loading) {
      return <div>Loading...</div>
    }

    if (!authenticated) {
      return null // Render nothing while redirecting
    }

    return <WrappedComponent {...props} />
  }
}

export default withAuth
