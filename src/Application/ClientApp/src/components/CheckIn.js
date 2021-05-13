import React, { Component } from 'react'
import 'bootstrap/dist/css/bootstrap.css'
import { Grid } from '@material-ui/core'
import {
	getEventId,
	getSelectedOptionsFromStorage,
	getStateFromLocalStorage,
} from './Common'
import { withAuth } from '../auth/MsalAuthProvider'
import { CheckInOptions } from './CheckIn/CheckInOptions'
import { CheckInInput } from './CheckIn/CheckInInput'
import { CheckInAlert } from './CheckIn/CheckInAlert'
import { CheckInPhoneNumbers } from './CheckIn/CheckInPhoneNumbers'
import { CheckInCandidates } from './CheckIn/CheckInCandidates'
import {
	fetchLocationGroups,
	fetchParentPhoneNumbers,
	postCheckInOut,
	postPhoneNumbers,
	postSecurityCode,
	postUndo,
} from '../helpers/BackendClient'

const _ = require('lodash')

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
			fastCheckInOut: getStateFromLocalStorage('fastCheckInOut', true),
			singleCheckInOut: getStateFromLocalStorage(
				'singleCheckInOut',
				false
			),
			showPhoneNumbers: getStateFromLocalStorage(
				'showPhoneNumbers',
				true
			),
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

	renderOptions() {
		return this.state.loading ? (
			<p>
				<em>Loading...</em>
			</p>
		) : (
			<CheckInOptions
				onClick={this.selectCheckType}
				checkType={this.state.checkType}
				fastCheckInOut={this.state.fastCheckInOut}
				singleCheckInOut={this.state.singleCheckInOut}
				showPhoneNumbers={this.state.showPhoneNumbers}
				onCheckBoxChange={this.handleChange}
				onLocationChange={this.updateOptions}
				locations={this.state.locations}
				checkInOutLocations={this.state.checkInOutLocations}
			/>
		)
	}

	renderInput() {
		return this.state.loading ? (
			<div />
		) : (
			<CheckInInput
				inputRef={this.securityCodeInput}
				securityCode={this.state.securityCode}
				onChange={this.updateSecurityCode}
				onSubmit={this.submitSecurityCode}
				onClear={this.resetView}
			/>
		)
	}

	renderAlert() {
		return this.state.alert.text.length <= 0 ? (
			<div />
		) : (
			<CheckInAlert
				showUndoLink={this.state.lastActionAttendanceIds.length > 0}
				onUndo={this.undoAction}
				alert={this.state.alert}
			/>
		)
	}

	renderCandidates() {
		return this.state.checkInOutCandidates.length <= 0 ? (
			<div />
		) : (
			<CheckInCandidates
				candidates={this.state.checkInOutCandidates}
				isSingleCheckInOut={this.state.singleCheckInOut}
				invertSelectCandidate={this.invertSelectCandidate}
				checkType={this.state.checkType}
				onCheckInOutMultiple={this.checkInOutMultiple}
				onCheckInOutSingle={this.checkInOutSingle}
			/>
		)
	}

	renderAdults() {
		return this.state.adults.length <= 0 ? (
			<div />
		) : (
			<Grid item xs={12}>
				<CheckInPhoneNumbers
					adults={this.state.adults}
					onPrimaryContactChange={this.changePrimaryContact}
					onSave={this.savePhoneNumber}
					onEdit={this.setPhoneNumberIsEdit}
					onChange={this.updatePhoneNumber}
					phoneNumberEditFlags={this.state.phoneNumberEditFlags}
				/>
			</Grid>
		)
	}

	render() {
		const options = this.renderOptions()
		const input = this.renderInput()
		const alert = this.renderAlert()
		const candidates = this.renderCandidates()
		const adults = this.renderAdults()

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
				{input}
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
		const eventId = getEventId(e)
		const adults = this.state.adults
		const adult = _.find(adults, { peopleId: eventId })
		adult['phoneNumber'] = e.target.value

		this.setState({ adults: adults })
	}

	async submitSecurityCode() {
		this.setState({ adults: [] })

		const selectedLocationIds = this.state.checkInOutLocations.map(
			(l) => l.value
		)
		const json = await postSecurityCode(
			this.state.securityCode,
			selectedLocationIds,
			this.state.fastCheckInOut,
			this.state.checkType
		)

		if (json['successfulFastCheckout'] === true) {
			this.resetView(false, false)
			await this.loadPhoneNumbers()
			this.setState({
				alert: { level: json['alertLevel'], text: json['text'] },
				lastActionAttendanceIds: json['attendanceIds'] ?? [],
			})
		} else {
			const candidates = json['checkInOutCandidates'].map(function (el) {
				const o = Object.assign({}, el)
				o.isSelected = true

				return o
			})

			this.setState({
				alert: { level: json['alertLevel'], text: json['text'] },
				lastActionAttendanceIds: json['attendanceIds'] ?? [],
				checkInOutCandidates: candidates,
			})
		}
	}

	async checkInOutSingle(event) {
		const candidate = this.state.checkInOutCandidates.find(
			(c) => c['attendanceId'] === parseInt(event.target.id, 10)
		)
		const json = await postCheckInOut([candidate], this.state.checkType)
		await this.processCheckInOutResult(json)
	}

	async checkInOutMultiple() {
		const candidates = this.state.checkInOutCandidates.filter(
			(c) => c.isSelected
		)
		const json = await postCheckInOut(candidates, this.state.checkType)
		await this.processCheckInOutResult(json)
	}

	async undoAction() {
		const json = await postUndo(
			this.state.lastActionAttendanceIds,
			this.state.checkType
		)

		this.setState({
			alert: { level: json['alertLevel'], text: json['text'] },
			lastActionAttendanceIds: [],
		})
	}

	async loadPhoneNumbers() {
		if (
			this.state.checkType === 'CheckOut' ||
			!this.state.showPhoneNumbers
		) {
			return
		}

		const json = await fetchParentPhoneNumbers(
			this.state.lastActionAttendanceIds
		)

		const editMap = _.map(json, function (adult) {
			return { id: adult['peopleId'], isEdit: false }
		})
		this.setState({
			adults: json,
			phoneNumberEditFlags: editMap,
		})
	}

	async processCheckInOutResult(json) {
		this.setState({
			alert: { level: json['alertLevel'], text: json['text'] },
			lastActionAttendanceIds: json['attendanceIds'] ?? [],
		})
		this.resetView(false, false)
		await this.loadPhoneNumbers()
	}

	resetView(resetAlert = true, resetAdults = true) {
		this.focus()
		this.setState({
			checkInOutCandidates: [],
			securityCode: '',
		})

		if (resetAlert) {
			this.setState({ alert: { text: '', level: 1 } })
		}

		if (resetAdults) {
			this.setState({ adults: [] })
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

	async changePrimaryContact(event) {
		const id = getEventId(event)

		const adults = this.state.adults
		const primary = _.find(adults, { peopleId: id })

		if (primary['isPrimaryContact']) {
			primary['isPrimaryContact'] = false
		} else {
			_.forEach(adults, function (a) {
				a['isPrimaryContact'] = false
			})

			primary['isPrimaryContact'] = true
		}

		this.setState({ adults: adults })
		await postPhoneNumbers(adults, false)
		this.focus()
	}

	async savePhoneNumber(event) {
		const eventId = getEventId(event)
		const flags = this.state.phoneNumberEditFlags
		const flag = _.find(flags, { id: eventId })
		flag['isEdit'] = false

		this.setState({ phoneNumberEditFlags: flags })
		await postPhoneNumbers(this.state.adults, true)
		this.focus()
	}

	setPhoneNumberIsEdit(event) {
		const eventId = getEventId(event)
		const flags = this.state.phoneNumberEditFlags
		const flag = _.find(flags, { id: eventId })
		flag['isEdit'] = true

		this.setState({ phoneNumberEditFlags: flags })
	}
}

export const CheckInOut = withAuth(CheckIn)
