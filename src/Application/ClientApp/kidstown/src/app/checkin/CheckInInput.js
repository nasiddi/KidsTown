'use client'
import React from 'react'
import {createTheme, ThemeProvider} from '@mui/material'
import Grid from '@mui/material/Grid2';

import {LargeButton, StyledTextField} from '../components/Common'

const theme = createTheme({
    palette: {
        neutral: {
            main: '#64748B',
            contrastText: '#fff',
        },
    },
})

export function CheckInInput(props) {
    return (
        <Grid size={12} key={props}>
            <Grid
                container
                spacing={1}
                justifyContent="space-between"
                alignItems="center"
            >
                <Grid size={{md: 8, xs: 12}}>
                    <StyledTextField
                        inputRef={props.inputRef}
                        id="outlined-basic"
                        label="SecurityCode"
                        variant="outlined"
                        value={props.securityCode}
                        onChange={props.onChange}
                        fullWidth={true}
                        InputLabelProps={{
                            shrink: true,
                        }}
                        autoFocus
                    />
                </Grid>
                <Grid size={{md: 2, xs: 12}}>
                    <LargeButton
                        id={'search'}
                        name={'Search'}
                        color="primary"
                        onClick={props.onSubmit}
                    />
                </Grid>
                <Grid size={{md: 2, xs: 12}}>
                    <ThemeProvider theme={theme}>
                        <LargeButton
                            id={'clear'}
                            name={'Clear'}
                            color="neutral"
                            onClick={props.onClear}
                        />
                    </ThemeProvider>
                </Grid>
            </Grid>
        </Grid>
    )
}
