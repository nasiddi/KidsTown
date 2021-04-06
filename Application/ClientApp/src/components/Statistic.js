import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import {
	fetchLocationGroups,
	getFormattedDate,
	getSelectedEventFromStorage,
	getSelectedOptionsFromStorage,
	MultiSelect,
} from './Common'
import { Table } from 'reactstrap'
import { withAuth } from '../auth/MsalAuthProvider'

const _ = require('lodash')

class Statistic extends Component {
	static displayName = Statistic.name
	repeat

	constructor(props) {
		super(props)

		this.updateSelectedLocationGroups = this.updateSelectedLocationGroups.bind(
			this
		)

		this.state = {
			locationGroups: [],
			statisticLocationGroups: getSelectedOptionsFromStorage(
				'statisticLocationGroups',
				[]
			),
			singleLocations: [],
			multiLocations: [],
			statisticLocations: [],
			attendees: {},
			renderLocationSelect: false,
			loading: true,
		}
	}

	async componentDidMount() {
		const locationGroups = await fetchLocationGroups()
		this.setState({ locationGroups: locationGroups })
		await this.setLocations(this.state.statisticLocationGroups)
		this.setState({
			attendees: await this.fetchData(this.state.statisticLocations),
		})
		this.setState({ loading: false })
	}

	componentWillUnmount() {
		clearTimeout(this.repeat)
	}

	renderLocationGroupSelect() {
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
							name={'statisticLocationGroups'}
							onChange={this.updateSelectedLocationGroups}
							options={this.state.locationGroups}
							defaultOptions={this.state.statisticLocationGroups}
						/>
					</Grid>
				</Grid>
			</div>
		)
	}

	renderLocationSelect() {
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
							onChange={this.updateSelectedLocations}
							options={this.state.multiLocations}
							defaultOptions={this.state.statisticLocations}
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

		let locationSelect = <div />
		if (this.state.renderLocationSelect) {
			locationSelect = this.renderLocationSelect()
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
						{this.renderLocationGroupSelect()}
					</Grid>
					<Grid item xs={12}>
						{locationSelect}
					</Grid>
					<Grid item xs={12}>
						{this.renderCounts()}
					</Grid>
				</Grid>
			</div>
		)
	}

	async fetchData(multiLocations) {
		const locations = multiLocations.concat(this.state.singleLocations)

		return await fetch(
			`overview/event/${await getSelectedEventFromStorage()}/attendees/history`,
			{
				body: JSON.stringify(locations.map((l) => l.value)),
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
			}
		)
			.then((r) => r.json())
			.then((j) => {
				return j
			})
	}

	async fetchLocations(locationsGroups) {
		return await fetch(
			`configuration/events/${await getSelectedEventFromStorage()}/locations`,
			{
				body: JSON.stringify(locationsGroups.map((l) => l.value)),
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
			}
		)
			.then((r) => r.json())
			.then((j) => {
				return j
			})
	}

	updateSelectedLocationGroups = async (options, key) => {
		localStorage.setItem(key.name, JSON.stringify(options))
		await this.setLocations(options)

		await this.setState({
			[key.name]: options,
			attendees: await this.fetchData(this.state.statisticLocations),
		})
	}

	updateSelectedLocations = async (options, key) => {
		await this.setState({
			[key.name]: options,
			attendees: await this.fetchData(options),
		})
	}

	setLocations = async (locationGroups) => {
		const locations = await this.fetchLocations(locationGroups)

		const singleLocations = _.filter(locations, function (l) {
			return l['optionCount'] === 1
		})

		const multiLocations = _.filter(locations, function (l) {
			return l['optionCount'] > 1
		})

		this.setState({
			renderLocationSelect: multiLocations.length > 0,
			statisticLocations: _.flatMap(multiLocations, function (l) {
				return l.options
			}),
			singleLocations: _.flatMap(singleLocations, function (l) {
				return l.options
			}),
			multiLocations: _.flatMap(multiLocations, function (l) {
				return l.options
			}),
		})
	}
}

export const Statistics = withAuth(Statistic)
