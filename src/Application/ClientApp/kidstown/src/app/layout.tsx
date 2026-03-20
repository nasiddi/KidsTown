import type React from 'react'

import './globals.css'
import { AuthProvider } from './components/AuthContext'

const _metadata = {
  title: 'KidsTown',
  icons: {
    icon: 'https://cdn3.iconfinder.com/data/icons/education-209/64/plane-paper-toy-science-school-512.png',
  },
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  )
}
