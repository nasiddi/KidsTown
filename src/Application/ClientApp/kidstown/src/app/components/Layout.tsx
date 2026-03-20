import { Container } from '@mui/material'
import type React from 'react'

import { NavMenu } from '../components/NavMenu'

export const NarrowLayout = ({ children }: { children: React.ReactNode }) => {
  return (
    <>
      <NavMenu />
      <br />
      <Container maxWidth={'lg'} style={{ marginTop: '50px' }}>
        {children}
      </Container>
    </>
  )
}

export const WideLayout = ({ children }: { children: React.ReactNode }) => {
  return (
    <Container maxWidth={'xl'} style={{ marginTop: '50px' }}>
      {children}
    </Container>
  )
}
