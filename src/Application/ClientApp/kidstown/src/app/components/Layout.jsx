import React from 'react'
import { Container } from '@mui/material'
import { NavMenu } from '../components/NavMenu'

export const NarrowLayout = (props) => {
  return (
    <>
      <NavMenu />
      <br />
      <Container maxWidth={'lg'} style={{ marginTop: '50px' }}>
        {props.children}
      </Container>
    </>
  )
}

export const WideLayout = (props) => {
  return (
    <Container maxWidth={'xl'} style={{ marginTop: '50px' }}>
      {props.children}
    </Container>
  )
}
