import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import {
	getStringFromSession,
	getSelectedOptionsFromStorage,
	getLastSunday,
	MultiSelect,
	DatePick,
} from './Common'
import { withAuth } from '../auth/MsalAuthProvider'
import { fetchLocationGroups } from '../helpers/BackendClient'

class Options extends Component {
	static displayName = Options.name
	repeat

	constructor(props) {
		super(props)

		this.updateOptions = this.updateOptions.bind(this)
		this.updateDate = this.updateDate.bind(this)
		this.resetDate = this.resetDate.bind(this)

		this.state = {
			locationGroups: [],
			overviewLocationGroups: getSelectedOptionsFromStorage(
				'overviewLocationGroups',
				[]
			),
			loading: true,
			date: '',
		}
	}

	async componentDidMount() {
		const locationGroups = await fetchLocationGroups()
		this.setState({
			date: getStringFromSession(
				'overviewDate',
				getLastSunday().toISOString()
			),
		})
		this.setState({ locationGroups: locationGroups })
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
						<MultiSelect
							name={'overviewLocationGroups'}
							isMulti={true}
							onChange={this.updateOptions}
							options={this.state.locationGroups}
							defaultOptions={this.state.overviewLocationGroups}
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
