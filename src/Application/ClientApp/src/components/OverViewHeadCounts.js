import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import {
	getSelectedEventFromStorage,
	getStringFromSession,
	getSelectedOptionsFromStorage,
	getLastSunday,
} from './Common'
import { Table } from 'reactstrap'
import { withAuth } from '../auth/MsalAuthProvider'
import { fetchLocationGroups } from '../helpers/BackendClient'

class OverViewHeadCounts extends Component {
	static displayName = OverViewHeadCounts.name
	repeat

	constructor(props) {
		super(props)

		this.state = {
			headCounts: [],
			loading: true,
		}
	}

	async componentDidMount() {
		const locations = await fetchLocationGroups()
		this.setState({ locations: locations })
		await this.fetchData()
		this.setState({ loading: false })
	}

	componentWillUnmount() {
		clearTimeout(this.repeat)
	}

	renderCounts() {
		if (this.state.loading) {
			return <div />
		}

		const headCounts = this.state.headCounts

		let totalCount = <tr />
		if (headCounts.length !== 1) {
			totalCount = (
				<tr key="Total">
					<th>Total</th>
					<th align="right">{this.GetTotalCount(false)}</th>
					<th align="right">{this.GetTotalCount(true)}</th>
				</tr>
			)
		}

		return (
			<Table>
				<thead>
					<tr>
						<th>Location</th>
						<th>Kinder</th>
						<th>Betreuer</th>
					</tr>
				</thead>
				<tbody>
					{headCounts.map((row) => (
						<tr key={row['location']}>
							<td> {row['location']}</td>
							<td>{this.GetCount(row, false)}</td>
							<td>{this.GetCount(row, true)}</td>
						</tr>
					))}
					{totalCount}
				</tbody>
			</Table>
		)
	}

	GetCount(headCounts, isVolunteer) {
		if (isVolunteer) {
			return headCounts['volunteersCount']
		}

		return headCounts['kidsCount']
	}

	GetTotalCount(isVolunteer) {
		if (isVolunteer) {
			const locationCount = this.state.headCounts.map(
				(h) => h['volunteersCount']
			)

			return locationCount.reduce(this.sum, 0)
		}

		const locationCount = this.state.headCounts.map((h) => h['kidsCount'])

		return locationCount.reduce(this.sum, 0)
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
						<h3 />
					</Grid>
					<Grid item xs={12}>
						<h3>CheckedIn</h3>
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
			`overview/event/${await getSelectedEventFromStorage()}/attendees/headcounts?date=${getStringFromSession(
				'overviewDate',
				getLastSunday().toISOString()
			)}`,
			{
				body: JSON.stringify(
					getSelectedOptionsFromStorage('overviewLocations', []).map(
						(l) => l.value
					)
				),
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
			}
		)
			.then((r) => r.json())
			.then((j) => {
				this.setState({ headCounts: j })
			})

		this.repeat = setTimeout(this.fetchData.bind(this), 500)
	}

	sum = (a, b) => {
		return a + b
	}
}

export const OverviewHeadCount = withAuth(OverViewHeadCounts)
