import React, { useState } from 'react'
import { Alert, Box, Button, Container, TextField } from '@mui/material'
import Grid from '@mui/material/Grid'
import CryptoJS from 'crypto-js'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useAuth } from '../components/AuthContext'
import { NarrowLayout } from '../components/Layout'

export default function Login() {
  return (
    <NarrowLayout>
      <LoginComponent />
    </NarrowLayout>
  )
}

function LoginComponent() {
  const [state, setState] = useState({
    username: '',
    hashedPassword: '',
    loginFailed: false,
  })

  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { login, loading } = useAuth()

  async function onEnterPress(event) {
    if (event.key === 'Enter') {
      await onLogin()
    }
  }

  async function onLogin() {
    localStorage.setItem('username', state.username)
    localStorage.setItem('passwordHash', state.hashedPassword)

    const loginSucceeded = await login(state.username, state.hashedPassword)

    if (loginSucceeded) {
      const redirectPath = searchParams.get('redirect') || '/'
      navigate(redirectPath, { replace: true })
      return
    }

    setState((prev) => ({ ...prev, loginFailed: true }))
  }

  function onUsernameChange(event) {
    setState((prev) => ({ ...prev, username: event.target.value }))
  }

  function onPasswordChange(event) {
    const password = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Utf8.parse(event.target.value))
    setState((prev) => ({ ...prev, hashedPassword: password }))
  }

  return (
    <Container maxWidth="xs">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          mt: 8,
        }}
      >
        <Grid container spacing={3}>
          <Grid size={12}>
            <TextField
              variant="outlined"
              label="UserName"
              value={state.username}
              name="username"
              onChange={onUsernameChange}
              onKeyDown={onEnterPress}
              fullWidth
            />
          </Grid>

          <Grid size={12}>
            <TextField
              type="password"
              variant="outlined"
              label="Password"
              name="password"
              onChange={onPasswordChange}
              onKeyDown={onEnterPress}
              fullWidth
            />
          </Grid>

          <Grid size={12}>
            <Button
              variant="contained"
              onClick={onLogin}
              fullWidth
              size="large"
              disableElevation
              disabled={loading}
            >
              Login
            </Button>
          </Grid>

          {state.loginFailed && (
            <Grid size={12}>
              <Alert severity="error">Login failed</Alert>
            </Grid>
          )}
        </Grid>
      </Box>
    </Container>
  )
}
