'use client'
import React, {useState} from 'react'
import {Alert, Box, Button, Container, TextField} from '@mui/material'
import Grid from '@mui/material/Grid2';
import CryptoJS from "crypto-js/"
import {NarrowLayout} from "@/app/components/Layout";
import {useRouter, useSearchParams} from 'next/navigation';
import {useAuth} from '../components/AuthContext';

export default function Login() {
    return (
        <NarrowLayout>
            <LoginComponent/>
        </NarrowLayout>
    )
}

function LoginComponent() {
    const initialState = {
        username: '',
        hashedPassword: '',
        loginFailed: false
    }
    const [state, setState] = useState(initialState)
    const router = useRouter();
    const searchParams = useSearchParams()
    const {login, loading} = useAuth();

    async function onEnterPress(event) {
        if (event.key === 'Enter') {
            await onLogin()
        }
    }

    async function onLogin() {
        console.log(state.username)
        console.log(state.hashedPassword)
        localStorage.setItem('username', state.username)
        localStorage.setItem('passwordHash', state.hashedPassword)
        const loginSucceeded = await login(state.username, state.hashedPassword)

        if (loginSucceeded) {
            const redirectPath = searchParams.get("redirect") || '/';
            router.push(redirectPath);
            return
        }

        setState({...state, loginFailed: true})
    }

    function onUsernameChange(event) {
        setState({...state, username: event.target.value})
    }

    function onPasswordChange(event) {
        const password = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Utf8.parse(event.target.value))
        setState({...state, hashedPassword: password})
    }

    function renderLogin() {
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
                                variant={'outlined'}
                                label={'UserName'}
                                value={state.username}
                                name={'username'}
                                onChange={onUsernameChange}
                                onKeyDown={onEnterPress}
                                fullWidth={true}
                            />
                        </Grid>
                        <Grid size={12}>
                            <TextField
                                type="password"
                                variant={'outlined'}
                                label={'Password'}
                                name={'password'}
                                onChange={onPasswordChange}
                                onKeyDown={onEnterPress}
                                fullWidth={true}
                            />
                        </Grid>
                        <Grid size={12}>
                            <Button
                                variant="contained"
                                color="primary"
                                onClick={onLogin}
                                fullWidth={true}
                                size={'large'}
                                disableElevation
                                disabled={loading}
                            >
                                Login
                            </Button>
                        </Grid>
                        {state.loginFailed ?
                            (<Grid size={12}>
                                <Alert severity="error">Login failed</Alert>
                            </Grid>) : <></>}
                    </Grid>
                </Box>
            </Container>
        )
    }

    function render() {
        return renderLogin()
    }

    return render()
}