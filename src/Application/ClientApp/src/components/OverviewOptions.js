import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import {
	getStringFromSession,
	getSelectedOptionsFromStorage,
	getLastSunday,
	MultiSelect,
} from './Common'
import { withAuth } from '../auth/MsalAuthProvider'
import { fetchLocationGroups } from '../helpers/BackendClient'
import { Input } from 'reactstrap'

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
				getLastSunday().toISOString().substr(0, 10)
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
					justifyContent="space-between"
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
						<Input
							id="datepicker"
							bsSize="md"
							type="date"
							value={this.state.date}
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
					justifyContent="space-between"
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

	updateDate(event) {
		const value = event.target.value
		console.log(value)
		if (value === null) {
			return
		}

		sessionStorage.setItem('overviewDate', value)
		this.setState({ date: value })
	}
}
export const OverviewOptions = withAuth(Options)
