import React, { Component } from 'react'
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

class CheckInGuest extends Component {
	static displayName = CheckInGuest.name
	constructor(props) {
		super(props)

		this.securityCodeInput = React.createRef()

		this.checkin = this.checkin.bind(this)
		this.createGuest = this.createGuest.bind(this)
		this.resetView = this.resetView.bind(this)
		this.updateOptions = this.updateOptions.bind(this)
		this.undoAction = this.undoAction.bind(this)
		this.selectCreateOption = this.selectCreateOption.bind(this)

		this.state = {
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
		}
	}

	async componentDidMount() {
		const locationGroups = await this.fetchLocationGroups()
		this.setState({ locationGroups: locationGroups, loading: false })
	}

	renderForm() {
		const toggleButtons = [
			{
				label: 'Create',
				onClick: this.selectCreateOption,
				isSelected: this.state.createOption === 'Create',
			},
			{
				label: 'CheckIn',
				onClick: this.selectCreateOption,
				isSelected: this.state.createOption === 'CheckIn',
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
							onChange={this.updateOptions}
							options={this.state.locationGroups}
							value={this.state.guestCheckInLocationGroup}
							minHeight={56}
							borderColor={
								this.state.locationIsValid
									? '#bfbfbf'
									: '#FF0000'
							}
						/>
					</Grid>
					<Grid item md={6} xs={12}>
						<MuiThemeProvider theme={primaryTheme}>
							<TextField
								error={!this.state.securityCodeIsValid}
								inputRef={this.securityCodeInput}
								label="SecurityCode"
								variant="outlined"
								value={this.state.securityCode}
								onChange={this.updateSecurityCode}
								fullWidth={true}
							/>
						</MuiThemeProvider>
					</Grid>
					<Grid item md={6} xs={12}>
						<MuiThemeProvider theme={primaryTheme}>
							<TextField
								error={!this.state.firstNameIsValid}
								label="FirstName"
								variant="outlined"
								value={this.state.firstName}
								onChange={this.updateFirstName}
								fullWidth={true}
							/>
						</MuiThemeProvider>
					</Grid>
					<Grid item md={6} xs={12}>
						<MuiThemeProvider theme={primaryTheme}>
							<TextField
								error={!this.state.lastNameIsValid}
								inputRef={this.securityCodeInput}
								label="LastName"
								variant="outlined"
								value={this.state.lastName}
								onChange={this.updateLastName}
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
								this.state.createOption === 'Create'
									? 'Create Guest'
									: 'Create and CheckIn Guest'
							}
							onClick={
								this.state.createOption === 'Create'
									? this.createGuest
									: this.checkin
							}
						/>
					</Grid>
				</Grid>
			</Grid>
		)
	}

	renderAlert() {
		const undoLink =
			this.state.lastActionAttendanceIds.length > 0 ? (
				<UndoButton callback={this.undoAction} />
			) : (
				<div />
			)

		return (
			<Grid item xs={12}>
				<Alert color={this.state.alert.level.toLowerCase()}>
					<Grid
						container
						direction="row"
						justifyContent="space-between"
						alignItems="center"
						spacing={1}
					>
						<Grid item xs={11}>
							{this.state.alert.text}
						</Grid>
						<Grid item xs={1}>
							{undoLink}
						</Grid>
					</Grid>
				</Alert>
			</Grid>
		)
	}

	render() {
		const form = this.state.loading ? (
			<p>
				<em>Loading...</em>
			</p>
		) : (
			this.renderForm()
		)

		const alert =
			this.state.alert.text.length > 0 ? this.renderAlert() : <div />

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

	updateOptions = (options, key) => {
		this.setState({ [key.name]: options })
	}

	updateSecurityCode = (e) => {
		this.setState({ securityCode: e.target.value })
	}

	updateFirstName = (e) => {
		this.setState({ firstName: e.target.value })
	}

	updateLastName = (e) => {
		this.setState({ lastName: e.target.value })
	}

	async checkin() {
		const isValid = await this.validateForm()

		if (!isValid) {
			return
		}

		await fetch('checkinout/guest/checkin', {
			body: JSON.stringify({
				securityCode: this.state.securityCode,
				locationId: this.state.guestCheckInLocationGroup.value,
				firstName: this.state.firstName,
				lastName: this.state.lastName,
			}),
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((r) => r.json())
			.then((j) => {
				this.setState({
					alert: { level: j['alertLevel'], text: j['text'] },
					lastActionAttendanceIds: j['attendanceIds'] ?? [],
				})
				this.resetView(false)
			})
	}

	async createGuest() {
		const isValid = await this.validateForm()

		if (!isValid) {
			return
		}

		await fetch('checkinout/guest/create', {
			body: JSON.stringify({
				securityCode: this.state.securityCode,
				locationId: this.state.guestCheckInLocationGroup.value,
				firstName: this.state.firstName,
				lastName: this.state.lastName,
			}),
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((r) => r.json())
			.then((j) => {
				this.setState({
					alert: { level: j['alertLevel'], text: j['text'] },
					lastActionAttendanceIds: j['attendanceIds'] ?? [],
				})
				this.resetView(false)
			})
	}

	async validateForm() {
		let isValid = true

		if (this.state.guestCheckInLocationGroup.value === 0) {
			isValid = false
			this.setState({ locationIsValid: false })
		} else {
			this.setState({ locationIsValid: true })
		}

		if (this.state.securityCode.length !== 4) {
			isValid = false
			this.setState({ securityCodeIsValid: false })
		} else {
			this.setState({ securityCodeIsValid: true })
		}

		if (this.state.firstName.length < 2) {
			isValid = false
			this.setState({ firstNameIsValid: false })
		} else {
			this.setState({ firstNameIsValid: true })
		}

		if (this.state.lastName.length < 2) {
			isValid = false
			this.setState({ lastNameIsValid: false })
		} else {
			this.setState({ lastNameIsValid: true })
		}

		return isValid
	}

	async undoAction() {
		await fetch('checkinout/undo/GuestCheckIn', {
			body: JSON.stringify(this.state.lastActionAttendanceIds),
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((r) => r.json())
			.then((j) => {
				this.setState({
					alert: { level: j['alertLevel'], text: j['text'] },
					lastActionAttendanceIds: [],
				})
			})
	}

	resetView(resetAlert = true) {
		this.setState({
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
			this.setState({ alert: { text: '', level: 1 } })
		}
	}

	async fetchLocationGroups() {
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

	selectCreateOption(event) {
		if (this.state.createOption !== event.target.id) {
			this.setState({ createOption: event.target.id })
			localStorage.setItem('createOption', event.target.id)
		}
	}
}

export const GuestCheckIn = withAuth(CheckInGuest)
