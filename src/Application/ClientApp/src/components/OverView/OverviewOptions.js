import React, { useState, useEffect } from 'react'
import { Grid, TextField } from '@mui/material'
import {
	getStringFromSession,
	getSelectedOptionsFromStorage,
	getLastSunday,
} from '../Common'
import { fetchLocationGroups } from '../../helpers/BackendClient'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import MultiSelect, { onDeselect, onSelect } from '../MultiSelect'

const optionKey = 'overviewLocationGroups'

export default function OverviewOptions() {
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

	function updateDate(value) {
		if (value === null) {
			return
		}

		const overviewDate = `${value.$y}-${value.$M + 1}-${value.$D}`
		sessionStorage.setItem('overviewDate', overviewDate)
		setState({ ...state, date: overviewDate })
	}

	function onLocationDeselect(event) {
		onDeselect(event, optionKey, state, setState)
	}

	function onLocationSelect(event) {
		onSelect(event, optionKey, state.locationGroups, state, setState)
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
							options={state.locationGroups}
							selectedOptions={state.overviewLocationGroups}
							onSelectOption={onLocationSelect}
							onRemoveOption={onLocationDeselect}
						/>
					</Grid>
					<Grid item xs={3}>
						<LocalizationProvider dateAdapter={AdapterDayjs}>
							<DatePicker
								style={{ color: 'black' }}
								value={state.date}
								inputFormat={'DD.MM.YYYY'}
								onChange={updateDate}
								renderInput={(params) => (
									<TextField {...params} />
								)}
							/>
						</LocalizationProvider>
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
