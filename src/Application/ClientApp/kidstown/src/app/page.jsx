import { ThemeProvider } from '@mui/material'
import { paletteOverrideTheme } from './components/Common'
import Documentation from './documentation/page'

export default function Home() {
  return (
    <div>
      <ThemeProvider theme={paletteOverrideTheme}>
        <Documentation />
      </ThemeProvider>
    </div>
  )
}
