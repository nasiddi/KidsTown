import { ThemeProvider } from '@mui/material'

import { paletteOverrideTheme } from './components/CommonHelpers'
import Documentation from './documentation/Documentation'

export default function Home() {
  return (
    <div>
      <ThemeProvider theme={paletteOverrideTheme}>
        <Documentation />
      </ThemeProvider>
    </div>
  )
}
