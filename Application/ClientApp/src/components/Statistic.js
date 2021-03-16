import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import {
	fetchLocations,
	getFormattedDate,
	getSelectedEventFromStorage,
	getSelectedOptionsFromStorage,
	MultiSelect,
} from './Common'
import { Table } from 'reactstrap'
import { withAuth } from '../auth/MsalAuthProvider'

class Statistic extends Component {
	static displayName = Statistic.name

	repeat

	constructor(props) {
		super(props)

		this.updateOptions = this.updateOptions.bind(this)

		this.state = {
			locations: [],
			statisticLocations: getSelectedOptionsFromStorage(
				'statisticLocations',
				[]
			),
			attendees: {},
			loading: true,
		}
	}

	async componentDidMount() {
		const locations = await fetchLocations()
		this.setState({ locations: locations })
		await this.fetchData()
		this.setState({ loading: false })
	}

	componentWillUnmount() {
		clearTimeout(this.repeat)
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
					<Grid item xs={12}>
						<MultiSelect
							name={'statisticLocations'}
							onChange={this.updateOptions}
							options={this.state.locations}
							defaultOptions={this.state.overviewLocations}
						/>
					</Grid>
				</Grid>
			</div>
		)
	}

	renderCounts() {
		if (this.state.loading) {
			return <div />
		}

		return (
			<Table>
				<thead>
					<tr>
						<th>Datum</th>
						<th>Kinder</th>
						<th>davon Gäste</th>
						<th>Betreuer</th>
						<th>kein CheckIn</th>
						<th>kein CheckOut</th>
					</tr>
				</thead>
				<tbody>
					{this.state.attendees
						.sort((a, b) => (a['date'] > b['date'] ? -1 : 1))
						.map((row) => (
							<tr key={row['date']}>
								<td>{getFormattedDate(row['date'])}</td>
								<td>
									{row['regularCount'] + row['guestCount']}
								</td>
								<td>{row['guestCount']}</td>
								<td>{row['volunteerCount']}</td>
								<td>{row['preCheckInOnlyCount']}</td>
								<td>{row['noCheckOutCount']}</td>
							</tr>
						))}
				</tbody>
			</Table>
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
					<Grid item xs={12}>
						{this.renderCounts()}
					</Grid>
				</Grid>
			</div>
		)
	}

	async fetchData() {
		await fetch(
			`overview/event/${await getSelectedEventFromStorage()}/attendees/history`,
			{
				body: JSON.stringify(
					this.state.statisticLocations.map((l) => l.value)
				),
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
			}
		)
			.then((r) => r.json())
			.then((j) => {
				this.setState({ attendees: j })
			})
	}

	updateOptions = async (options, key) => {
		localStorage.setItem(key.name, JSON.stringify(options))
		this.setState({ [key.name]: options })
		await this.fetchData()
	}
}

export const Statistics = withAuth(Statistic)
