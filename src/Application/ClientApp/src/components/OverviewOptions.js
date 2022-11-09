/* eslint-disable react/jsx-no-bind */
import React, { useState, useEffect } from 'react'
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

function Options() {
	const [state, setState] = useState({
		locationGroups: [],
		overviewLocationGroups: getSelectedOptionsFromStorage(
			'overviewLocationGroups',
			[]
		),
		loading: true,
		date: '',
	})

	useEffect(() => {
		async function load() {
			const locationGroups = await fetchLocationGroups()
			setState({
				...state,
				date: getStringFromSession(
					'overviewDate',
					getLastSunday().toISOString().substring(0, 10)
				),
				locationGroups: locationGroups,
				loading: false,
			})
		}

		load().then()
	}, [])

	const updateOptions = (options, key) => {
		localStorage.setItem(key.name, JSON.stringify(options))
		setState({ ...state, [key.name]: options })
	}

	function updateDate(event) {
		const value = event.target.value
		console.log(value)
		if (value === null) {
			return
		}

		sessionStorage.setItem('overviewDate', value)
		setState({ ...state, date: value })
	}

	function renderOptions() {
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
							onChange={updateOptions}
							options={state.locationGroups}
							defaultOptions={state.overviewLocationGroups}
							minHeight={0}
						/>
					</Grid>
					<Grid item xs={3}>
						<Input
							id="datepicker"
							bsSize="md"
							type="date"
							value={state.date}
							onChange={updateDate}
						/>
					</Grid>
				</Grid>
			</div>
		)
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
					{renderOptions()}
				</Grid>
			</Grid>
		</div>
	)
}
export const OverviewOptions = withAuth(Options)
