import React, { Component } from 'react'
import TextField from '@material-ui/core/TextField'
import 'bootstrap/dist/css/bootstrap.css'
import { Grid, MuiThemeProvider } from '@material-ui/core'
import {
	fetchLocationGroups,
	getSelectedEventFromStorage,
	getSelectedOptionsFromStorage,
	getStateFromLocalStorage,
	LocationSelect,
	primaryTheme,
	FAIcon,
	PrimaryCheckBox,
	ToggleButtons,
} from './Common'
import { Alert, Button } from 'reactstrap'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { withAuth } from '../auth/MsalAuthProvider'

const _ = require('lodash')

function UndoButton(props) {
	return (
		<a onClick={props['callback']} className="alert-link">
			<FAIcon name={'fas fa-undo-alt'} />
		</a>
	)
}

function SearchButton(props) {
	return (
		<Button
			size="lg"
			color={props['isSearch'] ? 'secondary' : 'primary'}
			block
			onClick={props['isSearch'] ? props['clear'] : props['submit']}
		>
			{props['isSearch'] ? 'Clear' : 'Search'}
		</Button>
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

class CheckIn extends Component {
	static displayName = CheckIn.name
	constructor(props) {
		super(props)

		this.securityCodeInput = React.createRef()

		this.resetView = this.resetView.bind(this)
		this.selectCheckType = this.selectCheckType.bind(this)
		this.submitSecurityCode = this.submitSecurityCode.bind(this)
		this.updateOptions = this.updateOptions.bind(this)
		this.checkInOutSingle = this.checkInOutSingle.bind(this)
		this.checkInOutMultiple = this.checkInOutMultiple.bind(this)
		this.invertSelectCandidate = this.invertSelectCandidate.bind(this)
		this.undoAction = this.undoAction.bind(this)
		this.handleChange = this.handleChange.bind(this)
		this.changePrimaryContact = this.changePrimaryContact.bind(this)
		this.setPhoneNumberIsEdit = this.setPhoneNumberIsEdit.bind(this)
		this.updatePhoneNumber = this.updatePhoneNumber.bind(this)
		this.savePhoneNumber = this.savePhoneNumber.bind(this)

		this.state = {
			locations: [],
			checkInOutCandidates: [],
			checkInOutLocations: getSelectedOptionsFromStorage(
				'checkInOutLocations',
				[]
			),
			securityCode: '',
			fastCheckInOut: getStateFromLocalStorage('fastCheckInOut'),
			singleCheckInOut: getStateFromLocalStorage('singleCheckInOut'),
			alert: { text: '', level: 1 },
			checkType: localStorage.getItem('checkType') ?? 'CheckIn',
			loading: true,
			lastActionAttendanceIds: [],
			adults: [],
			phoneNumberEditFlags: {},
		}
	}

	async componentDidMount() {
		const locations = await fetchLocationGroups()
		this.setState({ locations: locations, loading: false })
	}

	// eslint-disable-next-line no-unused-vars
	componentDidUpdate(prevProps, prevState, _) {
		if (
			prevState.securityCode.length !== 4 &&
			this.state.securityCode.length === 4
		) {
			this.submitSecurityCode().then()
		}
	}

	focus() {
		this.securityCodeInput.current.focus()
	}

	renderOptionsAndInput() {
		const toggleButtons = [
			{
				label: 'CheckIn',
				onClick: this.selectCheckType,
				isSelected: this.state.checkType === 'CheckIn',
			},
			{
				label: 'CheckOut',
				onClick: this.selectCheckType,
				isSelected: this.state.checkType === 'CheckOut',
			},
		]

		return (
			<Grid item xs={12}>
				<Grid
					container
					spacing={3}
					justify="space-between"
					alignItems="center"
				>
					<ToggleButtons buttons={toggleButtons} />
					<Grid item md={3} xs={6}>
						<PrimaryCheckBox
							name="fastCheckInOut"
							checked={this.state.fastCheckInOut}
							onChange={this.handleChange}
							label={`Fast ${this.state.checkType}`}
						/>
					</Grid>
					<Grid item md={3} xs={6}>
						<PrimaryCheckBox
							name="singleCheckInOut"
							checked={this.state.singleCheckInOut}
							onChange={this.handleChange}
							label={`Single ${this.state.checkType}`}
						/>
					</Grid>
					<Grid item md={3} xs={12}>
						<LocationSelect
							name={'checkInOutLocations'}
							onChange={this.updateOptions}
							isMulti={true}
							options={this.state.locations}
							defaultOptions={this.state.checkInOutLocations}
							minHeight={44}
						/>
					</Grid>
					<Grid item md={10} xs={12}>
						<MuiThemeProvider theme={primaryTheme}>
							<TextField
								inputRef={this.securityCodeInput}
								id="outlined-basic"
								label="SecurityCode"
								variant="outlined"
								value={this.state.securityCode}
								onChange={this.updateSecurityCode}
								fullWidth={true}
								autoFocus
							/>
						</MuiThemeProvider>
					</Grid>
					<Grid item md={2} xs={12}>
						<SearchButton
							isSearch={
								this.state.checkInOutCandidates.length > 0
							}
							clear={this.resetView}
							submit={this.submitSecurityCode}
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
						justify="space-between"
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

	renderSingleCheckout(checkInOutCandidates) {
		const candidates = checkInOutCandidates.map((c) => (
			<Grid item xs={12} key={c['attendanceId']}>
				<LargeButton
					id={c['attendanceId']}
					name={c['name']}
					color={this.getNameButtonColor(c)}
					onClick={this.checkInOutSingle}
				/>
			</Grid>
		))

		return (
			<Grid
				container
				spacing={3}
				justify="space-between"
				alignItems="center"
			>
				{candidates}
			</Grid>
		)
	}

	renderMultiCheckout(checkInOutCandidates) {
		const candidates = checkInOutCandidates.map((c) => (
			<Grid item xs={12} key={c['attendanceId']}>
				<LargeButton
					id={c['attendanceId']}
					name={c['name']}
					color={this.getNameButtonColor(c)}
					onClick={this.invertSelectCandidate}
					isOutline={!c.isSelected}
				/>
			</Grid>
		))

		return (
			<Grid container spacing={3}>
				{candidates}
				<Grid item xs={12}>
					<LargeButton
						id={'submit'}
						name={this.state.checkType}
						color={
							this.areNoCandidatesSelected()
								? 'secondary'
								: 'success'
						}
						onClick={this.checkInOutMultiple}
						isOutline={false}
						disabled={this.areNoCandidatesSelected()}
					/>
				</Grid>
			</Grid>
		)
	}

	renderAdults() {
		const adults = this.state.adults.map((a) => (
			<Grid item xs={12} key={a['personId']}>
				<Grid
					container
					spacing={3}
					justify="space-between"
					alignItems="center"
				>
					<Grid item xs={1}>
						<LargeButton
							id={a['personId']}
							name={
								<span id={a['personId']}>
									<FontAwesomeIcon icon="star" />
								</span>
							}
							color={
								a['isPrimaryContact'] ? 'success' : 'secondary'
							}
							isOutline={!a['isPrimaryContact']}
							onClick={this.changePrimaryContact}
						/>
					</Grid>
					<Grid item xs={6}>
						<h4
							style={{
								justifyContent: 'center',
								height: '100%',
								margin: 0,
							}}
						>
							{`${a['firstName']} ${a['lastName']}`}
						</h4>
					</Grid>
					{this.renderPhoneNumber(
						a['personId'],
						a['phoneNumber'],
						this.getPhoneNumberIsEdit(a['personId'])
					)}
				</Grid>
			</Grid>
		))

		return (
			<Grid
				container
				spacing={3}
				justify="space-between"
				alignItems="center"
			>
				{adults}
			</Grid>
		)
	}

	renderPhoneNumber(id, phoneNumber, isEdit) {
		if (isEdit) {
			const number = (
				<Grid item xs={3}>
					<MuiThemeProvider theme={primaryTheme}>
						<TextField
							id={id}
							label="PhoneNumber"
							variant="outlined"
							value={phoneNumber}
							fullWidth={true}
							onChange={this.updatePhoneNumber}
						/>
					</MuiThemeProvider>
				</Grid>
			)

			const button = (
				<Grid item xs={2}>
					<LargeButton
						id={id}
						name="Save"
						color="success"
						onClick={this.savePhoneNumber}
					/>
				</Grid>
			)

			return [number, button]
		}

		const number = (
			<Grid item xs={3}>
				<h4
					style={{
						justifyContent: 'center',
						height: '100%',
						margin: 0,
					}}
				>
					{phoneNumber}
				</h4>
			</Grid>
		)

		const button = (
			<Grid item xs={2}>
				<LargeButton
					id={id}
					name="Edit"
					color="primary"
					onClick={this.setPhoneNumberIsEdit}
				/>
			</Grid>
		)

		return [number, button]
	}

	render() {
		const options = this.state.loading ? (
			<p>
				<em>Loading...</em>
			</p>
		) : (
			this.renderOptionsAndInput()
		)

		const alert =
			this.state.alert.text.length > 0 ? this.renderAlert() : <div />

		let candidates = <div />
		if (this.state.singleCheckInOut) {
			candidates = (
				<Grid item xs={12}>
					{this.renderSingleCheckout(this.state.checkInOutCandidates)}
				</Grid>
			)
		} else if (this.state.checkInOutCandidates.length > 0) {
			candidates = this.renderMultiCheckout(
				this.state.checkInOutCandidates
			)
		}

		let adults = <div />
		if (this.state.adults.length > 0) {
			adults = (
				<Grid item xs={12}>
					{this.renderAdults()}
				</Grid>
			)
		}

		return (
			<Grid
				container
				spacing={3}
				justify="space-between"
				alignItems="center"
			>
				<Grid item xs={12}>
					<h1 id="title">{this.state.checkType}</h1>
				</Grid>
				{options}
				{alert}
				{candidates}
				{adults}
			</Grid>
		)
	}

	handleChange = (event) => {
		localStorage.setItem(event.target.name, event.target.checked)
		this.setState({ [event.target.name]: event.target.checked })
		this.focus()
	}

	updateOptions = (options, key) => {
		localStorage.setItem(key.name, JSON.stringify(options))
		this.setState({ [key.name]: options })
		this.resetView()
	}

	updateSecurityCode = (e) => {
		this.setState({ securityCode: e.target.value })
	}

	updatePhoneNumber = (e) => {
		const eventId = this.getEventId(e)
		const adults = this.state.adults
		const adult = _.find(adults, { personId: eventId })
		adult['phoneNumber'] = e.target.value

		this.setState({ adults: adults })
	}

	async submitSecurityCode() {
		this.setState({ adults: [] })

		const selectedLocationIds = this.state.checkInOutLocations.map(
			(l) => l.value
		)
		await fetch('checkinout/people', {
			body: JSON.stringify({
				securityCode: this.state.securityCode,
				eventId: await getSelectedEventFromStorage(),
				selectedLocationIds: selectedLocationIds,
				isFastCheckInOut: this.state.fastCheckInOut ?? false,
				checkType: this.state.checkType,
				attendanceIds: [],
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
				if (j['successfulFastCheckout'] === true) {
					this.resetView(false)
					this.loadPhoneNumbers()
				} else {
					const candidates = j['checkInOutCandidates'].map(function (
						el
					) {
						const o = Object.assign({}, el)
						o.isSelected = true

						return o
					})

					this.setState({ checkInOutCandidates: candidates })
				}
			})
	}

	async checkInOutSingle(event) {
		await fetch('checkinout/manual', {
			body: JSON.stringify({
				checkType: this.state.checkType,
				checkInOutCandidates: [
					this.state.checkInOutCandidates.find(
						(c) =>
							c['attendanceId'] === parseInt(event.target.id, 10)
					),
				],
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
				this.loadPhoneNumbers()
			})
	}

	async checkInOutMultiple() {
		const candidates = this.state.checkInOutCandidates.filter(
			(c) => c.isSelected
		)
		await fetch('checkinout/manual', {
			body: JSON.stringify({
				checkType: this.state.checkType,
				checkInOutCandidates: candidates,
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
				this.loadPhoneNumbers()
			})
	}

	async undoAction() {
		await fetch(`checkinout/undo/${this.state.checkType}`, {
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

	async loadPhoneNumbers() {
		if (this.state.checkType === 'CheckOut') {
			return
		}

		await fetch('people/adults', {
			body: JSON.stringify(this.state.lastActionAttendanceIds),
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((r) => r.json())
			.then((j) => {
				const editMap = _.map(j, function (adult) {
					return { id: adult['personId'], isEdit: false }
				})
				this.setState({
					adults: j,
					phoneNumberEditFlags: editMap,
				})
			})
	}

	async savePhoneNumbers(adults) {
		await fetch('people/adults/update', {
			body: JSON.stringify(adults),
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
		}).then()
	}

	resetView(resetAlert = true) {
		this.focus()
		this.setState({ checkInOutCandidates: [], securityCode: '' })

		if (resetAlert) {
			this.setState({ alert: { text: '', level: 1 } })
		}
	}

	invertSelectCandidate(event) {
		const candidate = this.state.checkInOutCandidates.find(
			(c) => c['attendanceId'] === parseInt(event.target.id, 10)
		)
		candidate.isSelected = !candidate.isSelected
		this.setState({ checkInOutCandidates: this.state.checkInOutCandidates })
	}

	areNoCandidatesSelected() {
		return (
			this.state.checkInOutCandidates.filter((c) => c.isSelected)
				.length <= 0
		)
	}

	checkTypeIsActive(checkType) {
		return !(this.state.checkType === checkType)
	}

	selectCheckType(event) {
		if (this.state.checkType !== event.target.id) {
			this.setState({ checkType: event.target.id })
			localStorage.setItem('checkType', event.target.id)
			this.resetView()
		}
	}

	getNameButtonColor(candidate) {
		if (this.state.checkType === 'CheckIn') {
			return 'primary'
		}

		if (candidate['hasPeopleWithoutPickupPermission']) {
			return 'danger'
		}

		if (!candidate['mayLeaveAlone']) {
			return 'warning'
		}

		return 'primary'
	}

	changePrimaryContact(event) {
		const id = this.getEventId(event)

		const adults = this.state.adults
		const primary = _.find(adults, { personId: id })

		if (primary['isPrimaryContact']) {
			primary['isPrimaryContact'] = false
		} else {
			_.forEach(adults, function (a) {
				a['isPrimaryContact'] = false
			})

			primary['isPrimaryContact'] = true
		}

		this.setState({ adults: adults })
		this.savePhoneNumbers(adults).then()
	}

	getEventId(event) {
		let id = parseInt(event.target.id, 10)
		if (isNaN(id)) {
			id = this.getElementId(event.target)
		}

		return id
	}

	getElementId(element) {
		const id = parseInt(element['parentElement']['id'], 10)
		if (!isNaN(id)) {
			return id
		}

		return this.getElementId(element['parentElement'])
	}

	savePhoneNumber(event) {
		const eventId = this.getEventId(event)
		const flags = this.state.phoneNumberEditFlags
		const flag = _.find(flags, { id: eventId })
		flag['isEdit'] = false

		this.setState({ phoneNumberEditFlags: flags })

		this.savePhoneNumbers(this.state.adults).then()
	}

	setPhoneNumberIsEdit(event) {
		const eventId = this.getEventId(event)
		const flags = this.state.phoneNumberEditFlags
		const flag = _.find(flags, { id: eventId })
		flag['isEdit'] = true

		this.setState({ phoneNumberEditFlags: flags })
	}

	getPhoneNumberIsEdit(personId) {
		const flag = _.find(this.state.phoneNumberEditFlags, { id: personId })

		return flag['isEdit']
	}
}

export const CheckInOut = withAuth(CheckIn)
