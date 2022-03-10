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
	fetchLocations,
	fetchParentPhoneNumbers,
	postChangeLocationAndCheckIn,
	postCheckInOut,
	postPhoneNumbers,
	postSecurityCode,
	postUndo,
} from '../helpers/BackendClient'
import { CheckInWithLocationChange } from './CheckIn/CheckInWithLocationChange'

const _ = require('lodash')

class CheckIn extends Component {
	static displayName = CheckIn.name
	constructor(props) {
		super(props)

		this.securityCodeInput = React.createRef()

		this.resetView = this.resetView.bind(this)
		this.selectCheckType = this.selectCheckType.bind(this)
		this.submitSecurityCodeWithLocationFilter =
			this.submitSecurityCodeWithLocationFilter.bind(this)
		this.submitSecurityCodeWithoutLocationFilter =
			this.submitSecurityCodeWithoutLocationFilter.bind(this)
		this.onChangeLocationGroupChange =
			this.onChangeLocationGroupChange.bind(this)
		this.onChangeLocationChange = this.onChangeLocationChange.bind(this)
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
		this.changeLocationAndCheckIn = this.changeLocationAndCheckIn.bind(this)

		this.state = {
			locationGroups: [],
			checkInOutCandidates: [],
			checkInOutLocationGroups: getSelectedOptionsFromStorage(
				'checkInOutLocationGroups',
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
			lastCodeSubmission: new Date(),
			showUnfilteredSearch: false,
			locations: [],
			selectedChangeLocation: {},
		}
	}

	async componentDidMount() {
		const locationGroups = await fetchLocationGroups()
		this.setState({ locationGroups: locationGroups, loading: false })
	}

	// eslint-disable-next-line no-unused-vars
	componentDidUpdate(prevProps, prevState, _) {
		if (
			prevState.securityCode.length !== 4 &&
			this.state.securityCode.length === 4
		) {
			const currentDate = new Date()
			if (currentDate - this.state.lastCodeSubmission > 2000) {
				this.setState({ lastCodeSubmission: new Date() })
				this.submitSecurityCodeWithLocationFilter().then()
			} else {
				this.resetView(false, false)
			}
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
				onLocationGroupChange={this.updateOptions}
				locationGroups={this.state.locationGroups}
				checkInOutLocationGroups={this.state.checkInOutLocationGroups}
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
				onSubmit={this.submitSecurityCodeWithLocationFilter}
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

	renderUnfilteredSearch() {
		if (this.state.checkType !== 'CheckIn') {
			return <div />
		}

		return (
			<CheckInWithLocationChange
				candidates={this.state.checkInOutCandidates}
				fadeIn={this.state.showUnfilteredSearch}
				onSearch={this.submitSecurityCodeWithoutLocationFilter}
				onLocationGroupChange={this.onChangeLocationGroupChange}
				onLocationChange={this.onChangeLocationChange}
				onSelectCandidate={this.selectCandidateForLocationChange}
				locationGroups={this.state.checkInOutLocationGroups}
				locations={this.state.locations['options'] ?? []}
				selectedLocation={this.state.selectedChangeLocation}
				onCheckIn={this.changeLocationAndCheckIn}
			/>
		)
	}

	render() {
		const options = this.renderOptions()
		const input = this.renderInput()
		const alert = this.renderAlert()

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
				{this.state.showUnfilteredSearch ? (
					<div />
				) : (
					this.renderCandidates()
				)}
				{this.state.showUnfilteredSearch ? (
					<div />
				) : (
					this.renderAdults()
				)}
				{this.state.showUnfilteredSearch ? (
					this.renderUnfilteredSearch()
				) : (
					<div />
				)}
			</Grid>
		)
	}

	async onChangeLocationChange(event) {
		this.setState({ selectedChangeLocation: event })
	}

	async onChangeLocationGroupChange(event) {
		const locations = await fetchLocations([event])
		this.setState({
			locations: locations[0],
			selectedChangeLocation:
				locations[0]['optionCount'] === 1
					? locations[0]['options'][0]
					: {},
		})
	}

	selectCandidateForLocationChange = (event) => {
		const candidate = this.state.checkInOutCandidates.find(
			(c) => c['attendanceId'] === parseInt(event.target.id, 10)
		)
		this.setState({ checkInOutCandidates: [candidate] })
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
		const adult = _.find(adults, { personId: eventId })
		adult['phoneNumber'] = e.target.value

		this.setState({ adults: adults })
	}

	async submitSecurityCodeWithoutLocationFilter() {
		const json = await postSecurityCode(
			this.state.securityCode,
			[],
			this.state.fastCheckInOut,
			this.state.checkType,
			false
		)

		this.setState({
			alert: { level: json['alertLevel'], text: json['text'] },
			lastActionAttendanceIds: [],
			checkInOutCandidates: json['checkInOutCandidates'],
		})
	}

	async submitSecurityCodeWithLocationFilter() {
		const selectedLocationGroupIds =
			this.state.checkInOutLocationGroups.map((l) => l.value)
		const json = await postSecurityCode(
			this.state.securityCode,
			selectedLocationGroupIds,
			this.state.fastCheckInOut,
			this.state.checkType,
			true
		)

		const attendanceIds = json['attendanceIds'] ?? []

		const state = {
			adults: [],
			alert: {
				level: json['alertLevel'],
				text: json['text'],
			},
			lastActionAttendanceIds: attendanceIds,
		}

		if (json['successfulFastCheckout'] === true) {
			this.resetView(false, false)
			this.setState({ ...state })
			await this.loadPhoneNumbers(attendanceIds)
		} else {
			const candidates = json['checkInOutCandidates'].map(function (el) {
				const o = Object.assign({}, el)
				o.isSelected = true

				return o
			})

			this.setState({
				...state,
				checkInOutCandidates: candidates,
				showUnfilteredSearch: json['filteredSearchUnsuccessful'],
			})
		}
	}

	async checkInOutMultiple() {
		const candidates = this.state.checkInOutCandidates.filter(
			(c) => c.isSelected
		)
		const json = await postCheckInOut(
			candidates,
			this.state.checkType,
			this.state.securityCode
		)
		await this.processCheckInOutResult(json)
	}

	async checkInOutSingle(event) {
		const candidate = this.state.checkInOutCandidates.find(
			(c) => c['attendanceId'] === parseInt(event.target.id, 10)
		)
		const json = await postCheckInOut(
			[candidate],
			this.state.checkType,
			this.state.securityCode
		)
		await this.processCheckInOutResult(json)
	}

	async changeLocationAndCheckIn() {
		const candidate = this.state.checkInOutCandidates[0]
		candidate.locationId = this.state.selectedChangeLocation.value
		const json = await postChangeLocationAndCheckIn(candidate)
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

	async loadPhoneNumbers(attendanceIds) {
		if (
			this.state.checkType === 'CheckOut' ||
			!this.state.showPhoneNumbers
		) {
			return
		}

		const json = await fetchParentPhoneNumbers(attendanceIds)

		const editMap = _.map(json, function (adult) {
			return { id: adult['personId'], isEdit: false }
		})
		this.setState({
			adults: json,
			phoneNumberEditFlags: editMap,
		})
	}

	async processCheckInOutResult(json) {
		const attendanceIds = json['attendanceIds'] ?? []
		this.setState({
			alert: { level: json['alertLevel'], text: json['text'] },
			lastActionAttendanceIds: attendanceIds,
		})
		this.resetView(false, false)
		await this.loadPhoneNumbers(attendanceIds)
	}

	resetView(resetAlert = true, resetAdults = true) {
		this.focus()
		let newState = {
			checkInOutCandidates: [],
			securityCode: '',
			showUnfilteredSearch: false,
			locations: [],
			selectedChangeLocation: {},
		}

		if (resetAlert) {
			newState = { ...newState, alert: { text: '', level: 1 } }
		}

		if (resetAdults) {
			newState = { ...newState, adults: [] }
		}

		this.setState({ ...newState })
	}

	invertSelectCandidate(event) {
		const candidate = this.state.checkInOutCandidates.find(
			(c) => c['attendanceId'] === parseInt(event.target.id, 10)
		)
		candidate.isSelected = !candidate.isSelected
		this.setState({ checkInOutCandidates: this.state.checkInOutCandidates })
	}

	selectCheckType(event) {
		if (this.state.checkType !== event.target.id) {
			this.setState({ checkType: event.target.id })
			localStorage.setItem('checkType', event.target.id)
			this.resetView()
		}
	}

	async changePrimaryContact(event) {
		const id = getEventId(event)

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
