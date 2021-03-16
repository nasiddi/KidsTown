import React from 'react'
import { Container } from 'reactstrap'
import { NavMenu } from './NavMenu'

export const CheckInLayout = (props) => {
	return (
		<div>
			<NavMenu />
			<Container>{props.children}</Container>
		</div>
	)
}

export const OverviewLayout = (props) => {
	return (
		<div>
			<Container className="themed-container" fluid={true}>
				{props.children}
			</Container>
		</div>
	)
}
