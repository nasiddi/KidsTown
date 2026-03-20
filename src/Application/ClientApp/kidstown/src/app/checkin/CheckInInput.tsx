import { createTheme, ThemeProvider } from '@mui/material'
import Grid from '@mui/material/Grid'
import type React from 'react'

import { LargeButton, StyledTextField } from '../components/Common'

declare module '@mui/material/styles' {
  interface Palette {
    neutral: Palette['primary']
  }
  interface PaletteOptions {
    neutral?: PaletteOptions['primary']
  }
}

declare module '@mui/material/Button' {
  interface ButtonPropsColorOverrides {
    neutral: true
  }
}

const theme = createTheme({
  palette: {
    neutral: {
      main: '#64748B',
      contrastText: '#fff',
    },
  },
})

interface CheckInInputProps {
  inputRef: React.RefObject<HTMLInputElement | null>
  securityCode: string
  onChange: React.ChangeEventHandler<HTMLInputElement>
  onSubmit: React.MouseEventHandler<HTMLButtonElement>
  onClear: React.MouseEventHandler<HTMLButtonElement>
}

export function CheckInInput({
  inputRef,
  securityCode,
  onChange,
  onSubmit,
  onClear,
}: CheckInInputProps) {
  return (
    <Grid size={12}>
      <Grid container spacing={1} justifyContent="space-between" alignItems="center">
        <Grid size={{ md: 8, xs: 12 }}>
          <StyledTextField
            inputRef={inputRef}
            id="outlined-basic"
            label="SecurityCode"
            variant="outlined"
            value={securityCode}
            onChange={onChange}
            fullWidth={true}
            InputLabelProps={{
              shrink: true,
            }}
            autoFocus
          />
        </Grid>
        <Grid size={{ md: 2, xs: 12 }}>
          <LargeButton id={'search'} name={'Search'} color="primary" onClick={onSubmit} />
        </Grid>
        <Grid size={{ md: 2, xs: 12 }}>
          <ThemeProvider theme={theme}>
            <LargeButton id={'clear'} name={'Clear'} color="neutral" onClick={onClear} />
          </ThemeProvider>
        </Grid>
      </Grid>
    </Grid>
  )
}
