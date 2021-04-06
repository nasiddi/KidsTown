import React, { Component } from 'react'
import TextField from '@material-ui/core/TextField'
import FormControlLabel from '@material-ui/core/FormControlLabel'
import Checkbox from '@material-ui/core/Checkbox'
import 'bootstrap/dist/css/bootstrap.css'
import { Grid, MuiThemeProvider } from '@material-ui/core'
import {
	fetchLocationGroups,
	getSelectedEventFromStorage,
	getSelectedOptionsFromStorage,
	getStateFromLocalStorage,
	MultiSelect,
	theme,
} from './Common'
import { Alert, Button, ButtonGroup } from 'reactstrap'
import { withAuth } from '../auth/MsalAuthProvider'

function CheckToggle(props) {
	return (
		<Grid item md={3} xs={12}>
			<ButtonGroup size="medium" color="primary">
				<Button
					id={'CheckIn'}
					onClick={props['callback']}
					color="primary"
					outline={!props['isCheckIn']}
				>
					CheckIn
				</Button>
				<Button
					id={'CheckOut'}
					onClick={props['callback']}
					color="primary"
					outline={props['isCheckIn']}
				>
					CheckOut
				</Button>
			</ButtonGroup>
		</Grid>
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
		return (
			<Grid item xs={12}>
				<Grid
					container
					spacing={3}
					justify="space-between"
					alignItems="center"
				>
					<CheckToggle
						isCheckIn={this.state.checkType === 'CheckIn'}
						callback={this.selectCheckType}
					/>
					<Grid item md={3} xs={6}>
						<MuiThemeProvider theme={theme}>
							<FormControlLabel
								control={
									<Checkbox
										name="fastCheckInOut"
										color="primary"
										checked={this.state.fastCheckInOut}
										onChange={this.handleChange}
									/>
								}
								label={`Fast ${this.state.checkType}`}
								labelPlacement="end"
							/>
						</MuiThemeProvider>
					</Grid>
					<Grid item md={3} xs={6}>
						<MuiThemeProvider theme={theme}>
							<FormControlLabel
								control={
									<Checkbox
										name="singleCheckInOut"
										color="primary"
										checked={this.state.singleCheckInOut}
										onChange={this.handleChange}
									/>
								}
								label={`Single ${this.state.checkType}`}
								labelPlacement="end"
							/>
						</MuiThemeProvider>
					</Grid>
					<Grid item md={3} xs={12}>
						<MultiSelect
							name={'checkInOutLocations'}
							onChange={this.updateOptions}
							options={this.state.locations}
							defaultOptions={this.state.checkInOutLocations}
						/>
					</Grid>
					<Grid item md={10} xs={12}>
						<MuiThemeProvider theme={theme}>
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
		return (
			<Grid item xs={12}>
				<Alert color={this.state.alert.level.toLowerCase()}>
					{this.state.alert.text}
				</Alert>
			</Grid>
		)
	}

	renderSingleCheckout(checkInOutCandidates) {
		const candidates = checkInOutCandidates.map((c) => (
			<Grid item xs={12} key={c['checkInId']}>
				<LargeButton
					id={c['checkInId']}
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
			<Grid item xs={12} key={c['checkInId']}>
				<LargeButton
					id={c['checkInId']}
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
			candidates = this.renderSingleCheckout(
				this.state.checkInOutCandidates
			)
		} else if (this.state.checkInOutCandidates.length > 0) {
			candidates = this.renderMultiCheckout(
				this.state.checkInOutCandidates
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
				<Grid item xs={12}>
					{candidates}
				</Grid>
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

	async submitSecurityCode() {
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
				checkInIds: [],
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
				})
				if (j['successfulFastCheckout'] === true) {
					this.resetView(false)
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
						(c) => c['checkInId'] === parseInt(event.target.id, 10)
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
				})
				this.resetView(false)
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
				})
				this.resetView(false)
			})
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
			(c) => c['checkInId'] === parseInt(event.target.id, 10)
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
}

export const CheckInOut = withAuth(CheckIn)
