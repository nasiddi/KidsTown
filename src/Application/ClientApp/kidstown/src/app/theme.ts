import { createTheme } from '@mui/material'

export const colors = {
  primary: '#047bff',
  tooltipBg: '#1c1c1f',
  tooltipText: 'rgba(255,255,255,0.87)',
  tooltipBorder: '#dadde9',
  tableBorder: 'rgba(0,0,0,0.54)',
  placeholder: '#bfbfbf',
}

export const paletteOverrideTheme = createTheme({
  palette: {
    success: {
      main: '#28a745',
      contrastText: '#fff',
    },
    info: {
      main: '#17a2b8',
      contrastText: '#fff',
    },
    secondary: {
      main: '#ffffff',
      contrastText: '#007bff',
    },
  },
})
