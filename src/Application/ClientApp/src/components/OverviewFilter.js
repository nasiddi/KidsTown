import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import {
	fetchLocationGroups,
	getStringFromSession,
	getSelectedOptionsFromStorage,
	getLastSunday,
	getSelectedFromSession,
} from './Common'
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
		this.updateDate = this.updateDate.bind(this)
		this.resetDate = this.resetDate.bind(this)

		this.state = {
			selectedStates: getSelectedFromSession('selectedOverviewStates', [
				'PreCheckedIn',
				'CheckedIn',
				'CheckedOut',
			]),
			locations: [],
			overviewLocations: getSelectedOptionsFromStorage(
				'overviewLocations',
				[]
			),
			loading: true,
			date: '',
		}
	}

	async componentDidMount() {
		const locations = await fetchLocationGroups()
		this.setState({
			date: getStringFromSession(
				'overviewDate',
				getLastSunday().toISOString()
			),
		})
		this.setState({ locations: locations })
		this.setState({ loading: false })
	}

	renderFilter() {
		return (
			<div>
				<Grid
					container
					spacing={3}
					justify="space-between"
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
		if (this.state.loading) {
			return <div />
		}

		return (
			<div>
				<Grid
					container
					spacing={3}
					justify="space-between"
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

	updateOptions = (options, key) => {
		localStorage.setItem(key.name, JSON.stringify(options))
		this.setState({ [key.name]: options })
	}

	resetDate() {
		const sunday = getLastSunday().toISOString()
		this.updateDate(sunday)
	}

	updateDate(value) {
		console.log(value)
		if (value === null) {
			return
		}

		sessionStorage.setItem('overviewDate', value)
		this.setState({ date: value })
	}
}

export const OverviewFilter = withAuth(Filter)
