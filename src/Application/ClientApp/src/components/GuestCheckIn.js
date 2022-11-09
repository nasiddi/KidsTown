/* eslint-disable react/jsx-no-bind */
import React, { useState, useEffect } from 'react'
import TextField from '@material-ui/core/TextField'
import 'bootstrap/dist/css/bootstrap.css'
import { Grid, MuiThemeProvider } from '@material-ui/core'
import {
	getSelectedEventFromStorage,
	MultiSelect,
	FAIcon,
	primaryTheme,
	ToggleButtons,
} from './Common'
import { Alert, Button } from 'reactstrap'
import { withAuth } from '../auth/MsalAuthProvider'

function UndoButton(props) {
	return (
		<a onClick={props['callback']} className="alert-link">
			<FAIcon name={'fas fa-undo-alt'} />
		</a>
	)
}

function LargeButton(props) {
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

function CheckInGuest() {
	const securityCodeInput = React.createRef()

	const [state, setState] = useState({
		locationGroups: [],
		guestCheckInLocationGroup: null,
		securityCode: '',
		firstName: '',
		lastName: '',
		alert: { text: '', level: 1 },
		loading: true,
		lastActionAttendanceIds: [],
		locationIsValid: true,
		securityCodeIsValid: true,
		firstNameIsValid: true,
		lastNameIsValid: true,
		createOption: localStorage.getItem('createOption') ?? 'Create',
	})

	useEffect(() => {
		async function load() {
			const locationGroups = await fetchLocationGroups()
			setState({
				...state,
				locationGroups: locationGroups,
				loading: false,
			})
		}

		load().then()
	}, [])

	const updateOptions = (options, key) => {
		setState({ ...state, [key.name]: options })
	}

	const updateSecurityCode = (e) => {
		setState({ ...state, securityCode: e.target.value })
	}

	const updateFirstName = (e) => {
		setState({ ...state, firstName: e.target.value })
	}

	const updateLastName = (e) => {
		setState({ ...state, lastName: e.target.value })
	}

	async function checkin() {
		const isValid = await validateForm()

		if (!isValid) {
			return
		}

		await fetch('checkinout/guest/checkin', {
			body: JSON.stringify({
				securityCode: state.securityCode,
				locationId: state.guestCheckInLocationGroup.value,
				firstName: state.firstName,
				lastName: state.lastName,
			}),
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((r) => r.json())
			.then((j) => {
				setState({
					...state,
					alert: { level: j['alertLevel'], text: j['text'] },
					lastActionAttendanceIds: j['attendanceIds'] ?? [],
				})
				resetView(false)
			})
	}

	async function createGuest() {
		const isValid = await validateForm()

		if (!isValid) {
			return
		}

		await fetch('checkinout/guest/create', {
			body: JSON.stringify({
				securityCode: state.securityCode,
				locationId: state.guestCheckInLocationGroup.value,
				firstName: state.firstName,
				lastName: state.lastName,
			}),
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((r) => r.json())
			.then((j) => {
				setState({
					...state,
					alert: { level: j['alertLevel'], text: j['text'] },
					lastActionAttendanceIds: j['attendanceIds'] ?? [],
				})
				resetView(false)
			})
	}

	async function validateForm() {
		let isValid = true

		if (state.guestCheckInLocationGroup.value === 0) {
			isValid = false
			setState({ ...state, locationIsValid: false })
		} else {
			setState({ ...state, locationIsValid: true })
		}

		if (state.securityCode.length !== 4) {
			isValid = false
			setState({ ...state, securityCodeIsValid: false })
		} else {
			setState({ ...state, securityCodeIsValid: true })
		}

		if (state.firstName.length < 2) {
			isValid = false
			setState({ ...state, firstNameIsValid: false })
		} else {
			setState({ ...state, firstNameIsValid: true })
		}

		if (state.lastName.length < 2) {
			isValid = false
			setState({ ...state, lastNameIsValid: false })
		} else {
			setState({ ...state, lastNameIsValid: true })
		}

		return isValid
	}

	async function undoAction() {
		await fetch('checkinout/undo/GuestCheckIn', {
			body: JSON.stringify(state.lastActionAttendanceIds),
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((r) => r.json())
			.then((j) => {
				setState({
					...state,
					alert: { level: j['alertLevel'], text: j['text'] },
					lastActionAttendanceIds: [],
				})
			})
	}

	function resetView(resetAlert = true) {
		setState({
			...state,
			securityCode: '',
			guestCheckInLocationGroup: null,
			firstName: '',
			lastName: '',
			locationIsValid: true,
			securityCodeIsValid: true,
			firstNameIsValid: true,
			lastNameIsValid: true,
		})

		if (resetAlert) {
			setState({ ...state, alert: { text: '', level: 1 } })
		}
	}

	async function fetchLocationGroups() {
		return await fetch(
			`configuration/events/${await getSelectedEventFromStorage()}/locationGroups`,
			{
				method: 'GET',
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

	function selectCreateOption(event) {
		if (state.createOption !== event.target.id) {
			setState({ ...state, createOption: event.target.id })
			localStorage.setItem('createOption', event.target.id)
		}
	}

	function renderForm() {
		const toggleButtons = [
			{
				label: 'Create',
				onClick: selectCreateOption,
				isSelected: state.createOption === 'Create',
			},
			{
				label: 'CheckIn',
				onClick: selectCreateOption,
				isSelected: state.createOption === 'CheckIn',
			},
		]

		return (
			<Grid item xs={12}>
				<Grid
					container
					spacing={3}
					justifyContent="space-between"
					alignItems="center"
				>
					<Grid item xs={12}>
						<ToggleButtons buttons={toggleButtons} />
					</Grid>
					<Grid item md={6} xs={12}>
						<MultiSelect
							name={'guestCheckInLocationGroup'}
							isMulti={false}
							onChange={updateOptions}
							options={state.locationGroups}
							value={state.guestCheckInLocationGroup}
							minHeight={56}
							borderColor={
								state.locationIsValid ? '#bfbfbf' : '#FF0000'
							}
						/>
					</Grid>
					<Grid item md={6} xs={12}>
						<MuiThemeProvider theme={primaryTheme}>
							<TextField
								error={!state.securityCodeIsValid}
								inputRef={securityCodeInput}
								label="SecurityCode"
								variant="outlined"
								value={state.securityCode}
								onChange={updateSecurityCode}
								fullWidth={true}
							/>
						</MuiThemeProvider>
					</Grid>
					<Grid item md={6} xs={12}>
						<MuiThemeProvider theme={primaryTheme}>
							<TextField
								error={!state.firstNameIsValid}
								label="FirstName"
								variant="outlined"
								value={state.firstName}
								onChange={updateFirstName}
								fullWidth={true}
							/>
						</MuiThemeProvider>
					</Grid>
					<Grid item md={6} xs={12}>
						<MuiThemeProvider theme={primaryTheme}>
							<TextField
								error={!state.lastNameIsValid}
								inputRef={securityCodeInput}
								label="LastName"
								variant="outlined"
								value={state.lastName}
								onChange={updateLastName}
								fullWidth={true}
							/>
						</MuiThemeProvider>
					</Grid>
					<Grid item xs={12}>
						<LargeButton
							id={'checkin'}
							block
							color={'success'}
							size="lg"
							name={
								state.createOption === 'Create'
									? 'Create Guest'
									: 'Create and CheckIn Guest'
							}
							onClick={
								state.createOption === 'Create'
									? createGuest
									: checkin
							}
						/>
					</Grid>
				</Grid>
			</Grid>
		)
	}

	function renderAlert() {
		const undoLink =
			state.lastActionAttendanceIds.length > 0 ? (
				<UndoButton callback={undoAction} />
			) : (
				<div />
			)

		return (
			<Grid item xs={12}>
				<Alert color={state.alert.level.toLowerCase()}>
					<Grid
						container
						direction="row"
						justifyContent="space-between"
						alignItems="center"
						spacing={1}
					>
						<Grid item xs={11}>
							{state.alert.text}
						</Grid>
						<Grid item xs={1}>
							{undoLink}
						</Grid>
					</Grid>
				</Alert>
			</Grid>
		)
	}

	const form = state.loading ? (
		<p>
			<em>Loading...</em>
		</p>
	) : (
		renderForm()
	)

	const alert = state.alert.text.length > 0 ? renderAlert() : <div />

	return (
		<Grid
			container
			spacing={3}
			justifyContent="space-between"
			alignItems="center"
		>
			<Grid item xs={12}>
				<h1 id="title">{'Guest CheckIn'}</h1>
			</Grid>
			{form}
			{alert}
		</Grid>
	)
}

export const GuestCheckIn = withAuth(CheckInGuest)
