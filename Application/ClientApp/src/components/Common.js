import { createMuiTheme } from '@material-ui/core'
import Select from 'react-select'
import React from 'react'
import DatePicker from 'reactstrap-date-picker'

export async function fetchLocations() {
	const response = await fetch('configuration/locations')

	return await response.json()
}

export const selectStyles = {
	menu: (base) => ({
		...base,
		zIndex: 100,
	}),
}

export function getSelectedOptionsFromStorage(key, fallback) {
	const s = localStorage.getItem(key)

	return s === null ? fallback : JSON.parse(s)
}

export function getStringFromSession(key, fallback) {
	const s = sessionStorage.getItem(key)

	return s === null ? fallback : s
}

export function getFormattedDate(dateString) {
	const date = new Date(dateString)

	return `${date.getUTCFullYear()}-${`0${date.getUTCMonth() + 1}`.slice(
		-2
	)}-${`0${date.getUTCDate()}`.slice(-2)}`
}

export const theme = createMuiTheme({
	palette: {
		primary: { main: '#047bff' },
	},
})

export function getStateFromLocalStorage(boolean) {
	const s = localStorage.getItem(boolean)

	return typeof s === 'undefined' ? false : JSON.parse(s)
}

export async function getSelectedEventFromStorage() {
	const s = localStorage.getItem('selectedEvent')

	if (s === null) {
		return await fetch('configuration/events/default')
			.then((r) => r.json())
			.then((j) => j['eventId'])
	} else {
		return JSON.parse(s)
	}
}

export function getLastSunday() {
	const t = new Date()
	t.setDate(t.getDate() - t.getDay())

	return t
}

export function MultiSelect(props) {
	return (
		<Select
			styles={selectStyles}
			isMulti
			placeholder="Select locations"
			name={props['name']}
			options={props['options']}
			className="basic-multi-select"
			classNamePrefix="select"
			onChange={props['onChange']}
			defaultValue={props['defaultOptions']}
			minWidth="100px"
		/>
	)
}

export function DatePick(props) {
	return (
		<DatePicker
			id="datepicker"
			value={props['value']}
			dateFormat="DD.MM.YYYY"
			onClear={props['onClear']}
			showClearButton
			onChange={props['onChange']}
		/>
	)
}
