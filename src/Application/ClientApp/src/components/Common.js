import React from 'react'
import { fetchDefaultEvent } from '../helpers/BackendClient'
import {
	Checkbox,
	FormControlLabel,
	Grid,
	Button,
	ButtonGroup,
	TextField,
	Tooltip,
	createTheme,
	TableCell,
} from '@mui/material'
import { styled } from '@mui/material/styles'

export const paletteOverrideTheme = createTheme({
	palette: {
		success: {
			main: '#28a745',
			contrastText: '#fff',
		},
		info: {
			main: '#17a2b8',
			contrastText: '#fff',
		},
		secondary: {
			main: '#ffffff',
			contrastText: '#007bff',
		},
	},
})

export function LargeButton(props) {
	return (
		<StyledButton
			id={props['id']}
			color={props['color']}
			onClick={props['onClick']}
			disabled={props['disabled']}
			fullWidth={true}
			variant={props['isOutline'] ? 'outlined' : 'contained'}
			size={'large'}
		>
			{props['name']}
		</StyledButton>
	)
}

export function getSelectedOptionsFromStorage(key, fallback) {
	const s = localStorage.getItem(key)

	return s === null ? fallback : JSON.parse(s)
}

export function getStringFromSession(key, fallback) {
	const s = sessionStorage.getItem(key)

	return s === null ? fallback : s
}

export function getSelectedFromSession(key, fallback) {
	const s = sessionStorage.getItem(key)

	return s === null ? fallback : JSON.parse(s)
}

export function getFormattedDate(dateString) {
	const date = new Date(dateString)

	return `${date.getUTCFullYear()}-${`0${date.getUTCMonth() + 1}`.slice(
		-2
	)}-${`0${date.getUTCDate()}`.slice(-2)}`
}

export function getStateFromLocalStorage(boolean, fallback) {
	const s = localStorage.getItem(boolean)

	return s === null ? fallback : JSON.parse(s)
}

export const StyledTextField = styled(TextField)({
	primary: { main: '#047bff' },
})

export const StyledButton = styled(Button)({
	secondary: '#047bff',
	height: 56,
})

export const StyledCheckBox = styled(Checkbox)({
	primary: { main: '#047bff' },
})

export async function getSelectedEventFromStorage() {
	const s = localStorage.getItem('selectedEvent')

	if (s === null) {
		return await fetchDefaultEvent()
	} else {
		return JSON.parse(s)
	}
}

export function getLastSunday() {
	const t = new Date()
	t.setDate(t.getDate() - t.getDay())

	return t
}

export function PrimaryCheckBox(props) {
	return (
		<FormControlLabel
			control={
				<StyledCheckBox
					name={props['name']}
					color="primary"
					checked={props['checked']}
					onChange={props['onChange']}
					disabled={props['disabled']}
				/>
			}
			label={props['label']}
			labelPlacement="end"
		/>
	)
}

export function ToggleButtons(props) {
	const buttons = props['buttons'].map((b) => (
		<StyledButton
			key={b['label']}
			id={b['label']}
			onClick={b['onClick']}
			color="primary"
			variant={!b['isSelected'] ? 'outlined' : 'contained'}
			size={'large'}
		>
			{b['label']}
		</StyledButton>
	))

	return (
		<Grid item md={3} xs={12}>
			<ButtonGroup size="medium" color="primary">
				{buttons}
			</ButtonGroup>
		</Grid>
	)
}

export function getGuid() {
	let deviceGuid = localStorage.getItem('deviceGuid')
	if (deviceGuid === null) {
		deviceGuid = createGuid()
		localStorage.setItem('deviceGuid', deviceGuid)
	}

	return deviceGuid
}

export function getEventId(event) {
	let id = parseInt(event.currentTarget.id, 10)
	if (isNaN(id)) {
		id = getId(event.currentTarget)
	}

	return id
}

export function getId(element) {
	const id = parseInt(element['parentElement']['id'], 10)
	if (!isNaN(id)) {
		return id
	}

	return getId(element['parentElement'])
}

export const HtmlTooltip = styled(Tooltip)({
	backgroundColor: '#1c1c1f',
	color: 'rgba(255,255,255,0.87)',
	maxWidth: 500,
	border: '1px solid #dadde9',
	fontsize: '30px',
})

export const TableHeadCell = styled(TableCell)({
	borderBottom: '1.5px solid rgba(0, 0, 0, 0.54)',
})

export const TableTotalCell = styled(TableCell)({
	borderBottom: '0px solid rgba(0, 0, 0, 0.54)',
	borderTop: '1.5px solid rgba(0, 0, 0, 0.54)',
})

function createGuid() {
	return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(
		/[xy]/g,
		function (c) {
			const r = (Math.random() * 16) | 0,
				v = c === 'x' ? r : (r & 0x3) | 0x8

			return v.toString(16)
		}
	)
}
