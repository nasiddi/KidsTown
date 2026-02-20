import React, { Suspense, lazy } from 'react'

const Login = lazy(() => import('../components/Login.jsx'))

export default function LoginPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <Login />
    </Suspense>
  )
}
