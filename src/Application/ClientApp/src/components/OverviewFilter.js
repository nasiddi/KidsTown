import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import { getSelectedFromSession } from './Common'
import { withAuth } from '../auth/MsalAuthProvider'
import { Button } from 'reactstrap'

const _ = require('lodash')

function FilterButton(props) {
	return (
		<Button
			color={props['color']}
			block
			onClick={props['onClick']}
			outline={!props['isSelected']}
		>
			{props['label']}
		</Button>
	)
}

class Filter extends Component {
	static displayName = Filter.name
	repeat

	constructor(props) {
		super(props)

		this.togglePreCheckedIn = this.togglePreCheckedIn.bind(this)
		this.toggleCheckedIn = this.toggleCheckedIn.bind(this)
		this.toggleCheckedOut = this.toggleCheckedOut.bind(this)
		this.toggleState = this.toggleState.bind(this)

		this.state = {
			selectedStates: getSelectedFromSession('selectedOverviewStates', [
				'PreCheckedIn',
				'CheckedIn',
				'CheckedOut',
			]),
		}
	}

	renderFilter() {
		return (
			<div>
				<Grid
					container
					spacing={3}
					justifyContent="space-between"
					alignItems="center"
				>
					<Grid item xs={12}>
						<h3 />
					</Grid>
					<Grid item md={4}>
						<FilterButton
							label="PreCheckedIn"
							color={'info'}
							onClick={this.togglePreCheckedIn}
							isSelected={_.includes(
								this.state.selectedStates,
								'PreCheckedIn'
							)}
						/>
					</Grid>
					<Grid item md={4}>
						<FilterButton
							label="CheckedIn"
							color={'success'}
							onClick={this.toggleCheckedIn}
							isSelected={_.includes(
								this.state.selectedStates,
								'CheckedIn'
							)}
						/>
					</Grid>
					<Grid item md={4}>
						<FilterButton
							label="CheckedOut"
							color={'primary'}
							onClick={this.toggleCheckedOut}
							isSelected={_.includes(
								this.state.selectedStates,
								'CheckedOut'
							)}
						/>
					</Grid>
				</Grid>
			</div>
		)
	}
	render() {
		return (
			<div>
				<Grid
					container
					spacing={3}
					justifyContent="space-between"
					alignItems="flex-start"
				>
					<Grid item xs={12}>
						{this.renderFilter()}
					</Grid>
				</Grid>
			</div>
		)
	}

	togglePreCheckedIn() {
		this.toggleState('PreCheckedIn')
	}

	toggleCheckedIn() {
		this.toggleState('CheckedIn')
	}

	toggleCheckedOut() {
		this.toggleState('CheckedOut')
	}

	toggleState(state) {
		// eslint-disable-next-line prefer-const
		let currentStates = this.state.selectedStates
		if (_.includes(currentStates, state)) {
			_.remove(currentStates, (p) => p === state)
		} else {
			currentStates.push(state)
		}

		this.setState({ selectedStates: currentStates })
		sessionStorage.setItem(
			'selectedOverviewStates',
			JSON.stringify(currentStates)
		)
	}
}

export const OverviewFilter = withAuth(Filter)
