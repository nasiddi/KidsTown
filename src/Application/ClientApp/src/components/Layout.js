import React from 'react'
import { Container } from '@mui/material'

export const NarrowLayout = (props) => {
	return <Container maxWidth={'lg'}>{props.children}</Container>
}

export const WideLayout = (props) => {
	return <Container maxWidth={'xl'}>{props.children}</Container>
}
