import React, { useState } from 'react'
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

function Filter() {
	const [state, setState] = useState({
		selectedStates: getSelectedFromSession('selectedOverviewStates', [
			'PreCheckedIn',
			'CheckedIn',
			'CheckedOut',
		]),
	})

	function renderFilter() {
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
							onClick={togglePreCheckedIn}
							isSelected={_.includes(
								state.selectedStates,
								'PreCheckedIn'
							)}
						/>
					</Grid>
					<Grid item md={4}>
						<FilterButton
							label="CheckedIn"
							color={'success'}
							onClick={toggleCheckedIn}
							isSelected={_.includes(
								state.selectedStates,
								'CheckedIn'
							)}
						/>
					</Grid>
					<Grid item md={4}>
						<FilterButton
							label="CheckedOut"
							color={'primary'}
							onClick={toggleCheckedOut}
							isSelected={_.includes(
								state.selectedStates,
								'CheckedOut'
							)}
						/>
					</Grid>
				</Grid>
			</div>
		)
	}

	function togglePreCheckedIn() {
		toggleState('PreCheckedIn')
	}

	function toggleCheckedIn() {
		toggleState('CheckedIn')
	}

	function toggleCheckedOut() {
		toggleState('CheckedOut')
	}

	function toggleState(toggledState) {
		// eslint-disable-next-line prefer-const
		let currentStates = state.selectedStates
		if (_.includes(currentStates, toggledState)) {
			_.remove(currentStates, (p) => p === toggledState)
		} else {
			currentStates.push(toggledState)
		}

		setState({ ...state, selectedStates: currentStates })
		sessionStorage.setItem(
			'selectedOverviewStates',
			JSON.stringify(currentStates)
		)
	}

	return (
		<div>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
				alignItems="flex-start"
			>
				<Grid item xs={12}>
					{renderFilter()}
				</Grid>
			</Grid>
		</div>
	)
}

export const OverviewFilter = withAuth(Filter)
