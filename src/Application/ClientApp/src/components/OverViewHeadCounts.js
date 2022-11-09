import React, { useState, useEffect } from 'react'
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

function OverViewHeadCounts() {
	let repeat = undefined

	const [state, setState] = useState({
		headCounts: [],
		loading: true,
	})

	useEffect(() => {
		async function load() {
			const locationGroups = await fetchLocationGroups()
			setState({ ...state, locationGroups: locationGroups })
			await fetchData()
			setState({ ...state, loading: false })
		}

		load().then()

		return () => {
			clearTimeout(repeat)
		}
	}, [])

	const sum = (a, b) => {
		return a + b
	}

	function renderCounts() {
		if (state.loading) {
			return <div />
		}

		const headCounts = state.headCounts

		let totalCount = <tr />
		if (headCounts.length !== 1) {
			totalCount = (
				<tr key="Total">
					<th>Total</th>
					<th align="right">{getTotalCount(false)}</th>
					<th align="right">{getTotalCount(true)}</th>
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
							<td>{getCount(row, false)}</td>
							<td>{getCount(row, true)}</td>
						</tr>
					))}
					{totalCount}
				</tbody>
			</Table>
		)
	}

	function getCount(headCounts, isVolunteer) {
		if (isVolunteer) {
			return headCounts['volunteersCount']
		}

		return headCounts['kidsCount']
	}

	function getTotalCount(isVolunteer) {
		if (isVolunteer) {
			const locationCount = state.headCounts.map(
				(h) => h['volunteersCount']
			)

			return locationCount.reduce(sum, 0)
		}

		const locationCount = state.headCounts.map((h) => h['kidsCount'])

		return locationCount.reduce(sum, 0)
	}

	async function fetchData() {
		await fetch(
			`overview/event/${await getSelectedEventFromStorage()}/attendees/headcounts?date=${getStringFromSession(
				'overviewDate',
				getLastSunday().toISOString()
			)}`,
			{
				body: JSON.stringify(
					getSelectedOptionsFromStorage(
						'overviewLocationGroups',
						[]
					).map((l) => l.value)
				),
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
			}
		)
			.then((r) => r.json())
			.then((j) => {
				setState({ ...state, headCounts: j })
			})

		repeat = setTimeout(fetchData.bind(this), 500)
	}

	if (state.loading) {
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
					<h3 />
				</Grid>
				<Grid item xs={12}>
					<h3>CheckedIn</h3>
				</Grid>
				<Grid item xs={12}>
					{renderCounts()}
				</Grid>
			</Grid>
		</div>
	)
}

export const OverviewHeadCount = withAuth(OverViewHeadCounts)
