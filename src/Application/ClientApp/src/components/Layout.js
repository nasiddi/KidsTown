import React from 'react'
import { Container } from '@mui/material'

export const NarrowLayout = (props) => {
	return (
		<Container maxWidth={'lg'} style={{ marginTop: '50px' }}>
			{props.children}
		</Container>
	)
}

export const WideLayout = (props) => {
	return (
		<Container maxWidth={'xl'} style={{ marginTop: '50px' }}>
			{props.children}
		</Container>
	)
}
