import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import {
	fetchLocationGroups,
	getStringFromSession,
	getSelectedOptionsFromStorage,
	getLastSunday,
	LocationSelect,
	DatePick,
} from './Common'
import { withAuth } from '../auth/MsalAuthProvider'

class Options extends Component {
	static displayName = Options.name
	repeat

	constructor(props) {
		super(props)

		this.updateOptions = this.updateOptions.bind(this)
		this.updateDate = this.updateDate.bind(this)
		this.resetDate = this.resetDate.bind(this)

		this.state = {
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

	renderOptions() {
		return (
			<div>
				<Grid
					container
					spacing={3}
					justify="space-between"
					alignItems="center"
				>
					<Grid item xs={9}>
						<LocationSelect
							name={'overviewLocations'}
							isMulti={true}
							onChange={this.updateOptions}
							options={this.state.locations}
							defaultOptions={this.state.overviewLocations}
							minHeight={0}
						/>
					</Grid>
					<Grid item xs={3}>
						<DatePick
							defaultValue={getLastSunday().toISOString()}
							value={this.state.date}
							onClear={this.resetDate}
							onChange={this.updateDate}
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
						{this.renderOptions()}
					</Grid>
				</Grid>
			</div>
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

export const OverviewOptions = withAuth(Options)
