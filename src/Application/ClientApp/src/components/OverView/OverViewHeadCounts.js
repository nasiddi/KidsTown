import React, { useState, useEffect } from 'react'
import {
	Grid,
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableRow,
} from '@mui/material'
import {
	getSelectedEventFromStorage,
	getStringFromSession,
	getSelectedOptionsFromStorage,
	getLastSunday,
	TableHeadCell,
	TableTotalCell,
} from '../Common'
import { fetchLocationGroups } from '../../helpers/BackendClient'

export default function OverviewHeadCount() {
	const [state, setState] = useState({
		headCounts: [],
		loading: true,
	})

	async function loadData() {
		const locationGroups = state.loading
			? await fetchLocationGroups()
			: state.locationGroups
		const headCounts = await fetchData()
		setState({
			...state,
			loading: false,
			locationGroups: locationGroups,
			headCounts: headCounts,
		})
	}

	const sum = (a, b) => {
		return a + b
	}

	useEffect(() => {
		const interval = setInterval(() => {
			loadData().then()
		}, 500)

		return () => clearInterval(interval)
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [])

	function renderCounts() {
		if (state.loading) {
			return <div />
		}

		const headCounts = state.headCounts

		let totalCount = <TableRow />
		if (headCounts.length !== 1) {
			totalCount = (
				<TableRow key="Total">
					<TableTotalCell>
						<strong>Total</strong>
					</TableTotalCell>
					<TableTotalCell>
						<strong>{getTotalCount(false)}</strong>
					</TableTotalCell>
					<TableTotalCell>
						<strong>{getTotalCount(true)}</strong>
					</TableTotalCell>
				</TableRow>
			)
		}

		return (
			<Table size="small">
				<TableHead>
					<TableRow>
						<TableHeadCell>
							<strong>Location</strong>
						</TableHeadCell>
						<TableHeadCell>
							<strong>Kinder</strong>
						</TableHeadCell>
						<TableHeadCell>
							<strong>Betreuer</strong>
						</TableHeadCell>
					</TableRow>
				</TableHead>
				<TableBody>
					{headCounts.map((row) => (
						<TableRow key={row['location']}>
							<TableCell> {row['location']}</TableCell>
							<TableCell>{getCount(row, false)}</TableCell>
							<TableCell>{getCount(row, true)}</TableCell>
						</TableRow>
					))}
					{totalCount}
				</TableBody>
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
		return await fetch(
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
		).then((r) => r.json())
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
					{/* eslint-disable-next-line */}
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
