import { createMuiTheme, Grid, MuiThemeProvider } from '@material-ui/core'
import Select from 'react-select'
import React from 'react'
import DatePicker from 'reactstrap-date-picker'
import { loadCSS } from 'fg-loadcss'
import Icon from '@material-ui/core/Icon'
import FormControlLabel from '@material-ui/core/FormControlLabel'
import Checkbox from '@material-ui/core/Checkbox'
import { Button, ButtonGroup } from 'reactstrap'

import { withStyles } from '@material-ui/styles'
import Tooltip from '@material-ui/core/Tooltip'
import { fetchDefaultEvent } from '../helpers/BackendClient'

function selectStyles(minHeight, borderColor) {
	return {
		menu: (base) => ({
			...base,
			zIndex: 100,
		}),
		control: (base) => ({
			...base,
			minHeight: minHeight ?? 0,
			borderColor: borderColor ?? '#bfbfbf',
		}),
	}
}

export function UndoButton(props) {
	return (
		<a onClick={props['callback']} className="alert-link">
			<FAIcon name={'fas fa-undo-alt'} />
		</a>
	)
}

export function LargeButton(props) {
	return (
		<Button
			id={props['id']}
			block
			color={props['color']}
			size="lg"
			onClick={props['onClick']}
			outline={props['isOutline']}
			disabled={props['disabled']}
		>
			{props['name']}
		</Button>
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

export const primaryTheme = createMuiTheme({
	palette: {
		primary: { main: '#047bff' },
	},
})

export function getStateFromLocalStorage(boolean, fallback) {
	const s = localStorage.getItem(boolean)

	return s === null ? fallback : JSON.parse(s)
}

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

export function MultiSelect(props) {
	return (
		<Select
			styles={selectStyles(props['minHeight'], props['borderColor'])}
			value={props['value']}
			isMulti={props['isMulti']}
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

export function PrimaryCheckBox(props) {
	return (
		<MuiThemeProvider theme={primaryTheme}>
			<FormControlLabel
				control={
					<Checkbox
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
		</MuiThemeProvider>
	)
}

export function ToggleButtons(props) {
	const buttons = props['buttons'].map((b) => (
		<Button
			key={b['label']}
			id={b['label']}
			onClick={b['onClick']}
			color="primary"
			outline={!b['isSelected']}
		>
			{b['label']}
		</Button>
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
		id = this.getElementId(event.currentTarget)
	}

	return id
}

export function getElementId(element) {
	const id = parseInt(element['parentElement']['id'], 10)
	if (!isNaN(id)) {
		return id
	}

	return this.getElementId(element['parentElement'])
}

export function FAIcon(props) {
	React.useEffect(() => {
		const node = loadCSS(
			'https://use.fontawesome.com/releases/v5.12.0/css/all.css',
			document.querySelector('#font-awesome-css')
		)

		return () => {
			node.parentNode.removeChild(node)
		}
	}, [])

	return <Icon className={props['name']} />
}

export const HtmlTooltip = withStyles(() => ({
	tooltip: {
		backgroundColor: '#1c1c1f',
		color: 'rgba(255,255,255,0.87)',
		maxWidth: 500,
		border: '1px solid #dadde9',
		fontsize: '30px',
	},
}))(Tooltip)

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
