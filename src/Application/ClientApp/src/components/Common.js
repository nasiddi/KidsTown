/* eslint-disable react/jsx-no-bind */
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

const _ = require('lodash')

export async function fetchLocationGroups() {
	const response = await fetch('configuration/location-groups')

	return await response.json()
}

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

export function LocationSelect(props) {
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

export function FontAwesomeIcon(props) {
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

// export function ToolTips(props) {
// 	const [tooltipOpen, setTooltipOpen] = useState(false)
//
// 	const toggle = () => setTooltipOpen(!tooltipOpen)
//
// 	return (
// 		<div>
// 			<Tooltip
// 				placement="top"
// 				isOpen={tooltipOpen}
// 				target={props['target']}
// 				toggle={toggle}
// 			>
// 				{_.join(props['lines'], '\n')}
// 			</Tooltip>
// 		</div>
// 	)
// }

export const HtmlTooltip = withStyles((theme) => ({
	tooltip: {
		backgroundColor: '#1c1c1f',
		color: 'rgba(255,255,255,0.87)',
		maxWidth: 500,
		border: '1px solid #dadde9',
		fontsize: '30px',
	},
}))(Tooltip)
