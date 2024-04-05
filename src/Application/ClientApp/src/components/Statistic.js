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
	getFormattedDate,
	getSelectedEventFromStorage,
	getSelectedOptionsFromStorage,
	LargeButton,
	TableHeadCell,
} from './Common'
import { fetchLocationGroups, fetchLocations } from '../helpers/BackendClient'
import { withAuth } from '../auth/MsalAuthProvider'
import { NarrowLayout } from './Layout'
import MultiSelect, { getOnDeselectId } from './MultiSelect'
import dayjs from 'dayjs'

function Statistic() {
	const [state, setState] = useState({
		locationGroups: [],
		allLocations: [],
		selectedLocations: [],
		attendees: [],
		renderLocationSelect: false,
		loading: true,
		loadingData: true,
		furthestYear: dayjs().year(),
	})

	useEffect(() => {
		async function load() {
			const allLocationGroups = await fetchLocationGroups()

			const selectedLocations = getSelectedOptionsFromStorage(
				'statisticLocations',
				[]
			)
			setState({
				...state,
				allLocationGroups: allLocationGroups,
				allLocations: await fetchLocations(),
				selectedLocations: selectedLocations,
				loading: false,
			})
		}
		load().then()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [])

	useEffect(() => {
		async function fetchData(locations) {
			if (locations.length === 0) {
				return []
			}

			return await fetch(
				`overview/event/${await getSelectedEventFromStorage()}/attendees/history?startDate=${
					state.furthestYear
				}-01-01&endDate=${dayjs().year()}-12-31`,
				{
					body: JSON.stringify(
						locations.flatMap((l) =>
							l.options
								.filter((o) => o.isSelected)
								.map((o) => o.value)
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
					return j
				})
		}
		async function load() {
			setState({
				...state,
				attendees: await fetchData(state.selectedLocations),
				loadingData: false,
			})
		}
		load().then()
		// eslint-disable-next-line
	}, [state.selectedLocations, state.furthestYear])

	function updateLocalStorage(locations) {
		localStorage.setItem('statisticLocations', JSON.stringify(locations))
	}

	function onLocationGroupSelect(event) {
		const locations = state.allLocations.find(
			(l) => l.groupId === parseInt(event.target.id, 10)
		)

		const selectedLocations = [...state.selectedLocations, locations]
		updateLocalStorage(selectedLocations)

		setState({
			...state,
			selectedLocations: selectedLocations,
		})
	}

	function onLocationGroupDeselect(event) {
		const id = getOnDeselectId(event)

		const selectedLocations = state.selectedLocations.filter(
			(l) => l.groupId !== parseInt(id, 10)
		)

		updateLocalStorage(selectedLocations)

		setState({
			...state,
			selectedLocations: selectedLocations,
		})
	}

	function updateOption(id, isSelected) {
		const locations = state.selectedLocations.find((l) => {
			return l.options.some((o) => o.value === id)
		})

		locations.options.find((o) => o.value === id).isSelected = isSelected

		updateLocalStorage(state.selectedLocations)

		setState({
			...state,
			selectedLocations: [...state.selectedLocations],
		})
	}

	function onLocationSelect(event) {
		const id = parseInt(event.target.id, 10)
		updateOption(id, true)
	}

	function onLocationDeselect(event) {
		const id = getOnDeselectId(event)
		updateOption(id, false)
	}

	function loadAnotherYear() {
		setState({
			...state,
			furthestYear: state.furthestYear - 1,
			loadingData: true,
		})
	}

	function renderLocationGroupSelect() {
		const selectedGroups = state.allLocationGroups.filter((g) =>
			state.selectedLocations.map((l) => l.groupId).includes(g.value)
		)

		return (
			<div>
				<Grid
					container
					spacing={3}
					justifyContent="space-between"
					alignItems="center"
				>
					<Grid item xs={12}>
						<MultiSelect
							options={state.allLocationGroups}
							selectedOptions={selectedGroups}
							onSelectOption={onLocationGroupSelect}
							onRemoveOption={onLocationGroupDeselect}
						/>
					</Grid>
				</Grid>
			</div>
		)
	}

	function renderLocationSelect() {
		return (
			<div>
				<Grid
					container
					spacing={3}
					justifyContent="space-between"
					alignItems="center"
				>
					<Grid item xs={12}>
						<MultiSelect
							options={state.selectedLocations
								.filter((l) => l.options.length > 1)
								.flatMap((l) =>
									l.options.filter((o) => !o.isSelected)
								)}
							selectedOptions={state.selectedLocations
								.filter((l) => l.options.length > 1)
								.flatMap((l) =>
									l.options.filter((o) => o.isSelected)
								)}
							onSelectOption={onLocationSelect}
							onRemoveOption={onLocationDeselect}
						/>
					</Grid>
				</Grid>
			</div>
		)
	}

	function renderCounts() {
		if (state.loading) {
			return <div />
		}

		return (
			<Table size={'small'}>
				<TableHead>
					<TableRow>
						<TableHeadCell>
							<strong>Datum</strong>
						</TableHeadCell>
						<TableHeadCell align="right">
							<strong>Kinder</strong>
						</TableHeadCell>
						<TableHeadCell align="right">
							<strong>davon Gäste</strong>
						</TableHeadCell>
						<TableHeadCell align="right">
							<strong>Betreuer</strong>
						</TableHeadCell>
						<TableHeadCell align="right">
							<strong>kein CheckIn</strong>
						</TableHeadCell>
						<TableHeadCell align="right">
							<strong>kein CheckOut</strong>
						</TableHeadCell>
					</TableRow>
				</TableHead>
				<TableBody>
					{state.attendees
						.sort((a, b) => (a['date'] > b['date'] ? -1 : 1))
						.map((row) => (
							<TableRow key={row['date']}>
								<TableCell>
									{getFormattedDate(row['date'])}
								</TableCell>
								<TableCell align="right">
									{row['regularCount'] + row['guestCount']}
								</TableCell>
								<TableCell align="right">
									{row['guestCount']}
								</TableCell>
								<TableCell align="right">
									{row['volunteerCount']}
								</TableCell>
								<TableCell align="right">
									{row['preCheckInOnlyCount']}
								</TableCell>
								<TableCell align="right">
									{row['noCheckOutCount']}
								</TableCell>
							</TableRow>
						))}
				</TableBody>
			</Table>
		)
	}

	if (state.loading) {
		return <div />
	}

	const locationSelect = state.selectedLocations.some(
		(s) => s.options.length > 1
	) ? (
		renderLocationSelect()
	) : (
		<div />
	)

	const noMoreDataAvailable = !state.attendees.some((a) =>
		a.date.startsWith(state.furthestYear)
	)

	return (
		<NarrowLayout>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
				alignItems="flex-start"
			>
				<Grid item xs={12}>
					{renderLocationGroupSelect()}
				</Grid>
				<Grid item xs={12}>
					{locationSelect}
				</Grid>
				<Grid item xs={12}>
					{renderCounts()}
				</Grid>
				<Grid item xs={12}>
					<LargeButton
						name={
							state.loadingData
								? 'loading'
								: noMoreDataAvailable 
									? 'No more data' 
									: `Load ${state.furthestYear - 1}`
						}
						onClick={loadAnotherYear}
						disabled={noMoreDataAvailable}
					/>
				</Grid>
			</Grid>
			<br />
		</NarrowLayout>
	)
}

export const Statistics = withAuth(Statistic)
