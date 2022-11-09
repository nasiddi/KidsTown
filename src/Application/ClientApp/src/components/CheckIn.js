/* eslint-disable react/jsx-no-bind */
import React, { useEffect, useState } from 'react'
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

const cleanState = {
	checkInOutCandidates: [],
	securityCode: '',
	showUnfilteredSearch: false,
	locations: [],
	selectedChangeLocation: {},
}

const clearedAlert = { alert: { text: '', level: 1 } }

const clearedAdults = { adults: [] }

function CheckIn() {
	const [state, setState] = useState({
		locationGroups: [],
		checkInOutCandidates: [],
		checkInOutLocationGroups: getSelectedOptionsFromStorage(
			'checkInOutLocationGroups',
			[]
		),
		securityCode: '',
		fastCheckInOut: getStateFromLocalStorage('fastCheckInOut', true),
		singleCheckInOut: getStateFromLocalStorage('singleCheckInOut', false),
		showPhoneNumbers: getStateFromLocalStorage('showPhoneNumbers', true),
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
	})

	const securityCodeInput = React.useRef(null)

	useEffect(() => {
		if (securityCodeInput.current) {
			securityCodeInput.current.focus()
		}
	}, [state])

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

	// eslint-disable-next-line no-unused-vars

	useEffect(() => {
		if (state.securityCode.length === 4) {
			const currentDate = new Date()
			if (currentDate - state.lastCodeSubmission > 2000) {
				setState({ ...state, lastCodeSubmission: new Date() })
				submitSecurityCodeWithLocationFilter().then()
			} else {
				setState({ ...state, ...cleanState })
			}
		}
	}, [state.securityCode])

	async function onChangeLocationChange(event) {
		setState({ ...state, selectedChangeLocation: event })
	}

	async function onChangeLocationGroupChange(event) {
		const locations = await fetchLocations([event])
		setState({
			...state,
			locations: locations[0],
			selectedChangeLocation:
				locations[0]['optionCount'] === 1
					? locations[0]['options'][0]
					: {},
		})
	}

	const selectCandidateForLocationChange = (event) => {
		const candidate = state.checkInOutCandidates.find(
			(c) => c['attendanceId'] === parseInt(event.target.id, 10)
		)
		setState({ ...state, checkInOutCandidates: [candidate] })
	}

	const handleChange = (event) => {
		localStorage.setItem(event.target.name, event.target.checked)
		setState({ ...state, [event.target.name]: event.target.checked })
	}

	const updateOptions = (options, key) => {
		localStorage.setItem(key.name, JSON.stringify(options))
		setState({
			...state,
			...cleanState,
			...clearedAlert,
			...clearedAdults,
			[key.name]: options,
		})
	}

	const updateSecurityCode = (e) => {
		setState({ ...state, securityCode: e.target.value })
	}

	const updatePhoneNumber = (e) => {
		const eventId = getEventId(e)
		const adults = state.adults
		const adult = _.find(adults, { personId: eventId })
		adult['phoneNumber'] = e.target.value

		setState({ ...state, adults: adults })
	}

	async function submitSecurityCodeWithoutLocationFilter() {
		const json = await postSecurityCode(
			state.securityCode,
			[],
			state.fastCheckInOut,
			state.checkType,
			false
		)

		setState({
			...state,
			alert: { level: json['alertLevel'], text: json['text'] },
			lastActionAttendanceIds: [],
			checkInOutCandidates: json['checkInOutCandidates'],
		})
	}

	async function submitSecurityCodeWithLocationFilter() {
		const selectedLocationGroupIds = state.checkInOutLocationGroups.map(
			(l) => l.value
		)
		const json = await postSecurityCode(
			state.securityCode,
			selectedLocationGroupIds,
			state.fastCheckInOut,
			state.checkType,
			true
		)

		const attendanceIds = json['attendanceIds'] ?? []

		const resetState = {
			adults: [],
			alert: {
				level: json['alertLevel'],
				text: json['text'],
			},
			lastActionAttendanceIds: attendanceIds,
		}

		if (json['successfulFastCheckout'] === true) {
			const result = await loadPhoneNumbers(attendanceIds)
			const newVar = {
				...state,
				...cleanState,
				...resetState,
				adults: result.adults,
				phoneNumberEditFlags: result.phoneNumberEditFlags,
			}
			setState(newVar)
		} else {
			const candidates = json['checkInOutCandidates'].map(function (el) {
				const o = Object.assign({}, el)
				o.isSelected = true

				return o
			})

			setState({
				...state,
				checkInOutCandidates: candidates,
				showUnfilteredSearch: json['filteredSearchUnsuccessful'],
			})
		}
	}

	async function checkInOutMultiple() {
		const candidates = state.checkInOutCandidates.filter(
			(c) => c.isSelected
		)
		const json = await postCheckInOut(
			candidates,
			state.checkType,
			state.securityCode
		)
		await processCheckInOutResult(json)
	}

	async function checkInOutSingle(event) {
		const candidate = state.checkInOutCandidates.find(
			(c) => c['attendanceId'] === parseInt(event.target.id, 10)
		)
		const json = await postCheckInOut(
			[candidate],
			state.checkType,
			state.securityCode
		)
		await processCheckInOutResult(json)
	}

	async function changeLocationAndCheckIn() {
		const candidate = state.checkInOutCandidates[0]
		candidate.locationId = state.selectedChangeLocation.value
		const json = await postChangeLocationAndCheckIn(candidate)
		await processCheckInOutResult(json)
	}

	async function undoAction() {
		const json = await postUndo(
			state.lastActionAttendanceIds,
			state.checkType
		)

		setState({
			...state,
			...cleanState,
			...clearedAdults,
			alert: { level: json['alertLevel'], text: json['text'] },
			lastActionAttendanceIds: [],
		})
	}

	async function loadPhoneNumbers(attendanceIds) {
		if (state.checkType === 'CheckOut' || !state.showPhoneNumbers) {
			return { adults: [], phoneNumberEditFlags: {} }
		}

		const json = await fetchParentPhoneNumbers(attendanceIds)

		const editMap = _.map(json, function (adult) {
			return { id: adult['personId'], isEdit: false }
		})

		return { adults: json, phoneNumberEditFlags: editMap }
	}

	async function processCheckInOutResult(json) {
		const attendanceIds = json['attendanceIds'] ?? []
		const result = await loadPhoneNumbers(attendanceIds)

		setState({
			...state,
			...cleanState,
			alert: { level: json['alertLevel'], text: json['text'] },
			lastActionAttendanceIds: attendanceIds,
			adults: result.adults,
			phoneNumberEditFlags: result.phoneNumberEditFlags,
		})
	}

	function resetView() {
		setState({ ...state, ...cleanState, ...clearedAlert, ...clearedAdults })
	}

	function invertSelectCandidate(event) {
		const candidate = state.checkInOutCandidates.find(
			(c) => c['attendanceId'] === parseInt(event.target.id, 10)
		)
		candidate.isSelected = !candidate.isSelected
		setState({ ...state, checkInOutCandidates: state.checkInOutCandidates })
	}

	function selectCheckType(event) {
		if (state.checkType !== event.target.id) {
			setState({
				...state,
				...cleanState,
				...clearedAlert,
				...clearedAdults,
				checkType: event.target.id,
			})
			localStorage.setItem('checkType', event.target.id)
		}
	}

	async function changePrimaryContact(event) {
		const id = getEventId(event)

		const adults = state.adults
		const primary = _.find(adults, { personId: id })

		if (primary['isPrimaryContact']) {
			primary['isPrimaryContact'] = false
		} else {
			_.forEach(adults, function (a) {
				a['isPrimaryContact'] = false
			})

			primary['isPrimaryContact'] = true
		}

		setState({ ...state, adults: adults })
		await postPhoneNumbers(adults, false)
	}

	async function savePhoneNumber(event) {
		const eventId = getEventId(event)
		const flags = state.phoneNumberEditFlags
		const flag = _.find(flags, { id: eventId })
		flag['isEdit'] = false

		setState({ ...state, phoneNumberEditFlags: flags })
		await postPhoneNumbers(state.adults, true)
	}

	function setPhoneNumberIsEdit(event) {
		const eventId = getEventId(event)
		const flags = state.phoneNumberEditFlags
		const flag = _.find(flags, { id: eventId })
		flag['isEdit'] = true

		setState({ ...state, phoneNumberEditFlags: flags })
	}

	function renderOptions() {
		return state.loading ? (
			<p>
				<em>Loading...</em>
			</p>
		) : (
			<CheckInOptions
				onClick={selectCheckType}
				checkType={state.checkType}
				fastCheckInOut={state.fastCheckInOut}
				singleCheckInOut={state.singleCheckInOut}
				showPhoneNumbers={state.showPhoneNumbers}
				onCheckBoxChange={handleChange}
				onLocationGroupChange={updateOptions}
				locationGroups={state.locationGroups}
				checkInOutLocationGroups={state.checkInOutLocationGroups}
			/>
		)
	}

	function renderInput() {
		return state.loading ? (
			<div />
		) : (
			<CheckInInput
				inputRef={securityCodeInput}
				securityCode={state.securityCode}
				onChange={updateSecurityCode}
				onSubmit={submitSecurityCodeWithLocationFilter}
				onClear={resetView}
			/>
		)
	}

	function renderAlert() {
		return state.alert.text.length <= 0 ? (
			<div />
		) : (
			<CheckInAlert
				showUndoLink={state.lastActionAttendanceIds.length > 0}
				onUndo={undoAction}
				alert={state.alert}
			/>
		)
	}

	function renderCandidates() {
		return state.checkInOutCandidates.length <= 0 ? (
			<div />
		) : (
			<CheckInCandidates
				candidates={state.checkInOutCandidates}
				isSingleCheckInOut={state.singleCheckInOut}
				invertSelectCandidate={invertSelectCandidate}
				checkType={state.checkType}
				onCheckInOutMultiple={checkInOutMultiple}
				onCheckInOutSingle={checkInOutSingle}
			/>
		)
	}

	function renderAdults() {
		return state.adults.length <= 0 ? (
			<div />
		) : (
			<Grid item xs={12}>
				<CheckInPhoneNumbers
					adults={state.adults}
					onPrimaryContactChange={changePrimaryContact}
					onSave={savePhoneNumber}
					onEdit={setPhoneNumberIsEdit}
					onChange={updatePhoneNumber}
					phoneNumberEditFlags={state.phoneNumberEditFlags}
				/>
			</Grid>
		)
	}

	function renderUnfilteredSearch() {
		if (state.checkType !== 'CheckIn') {
			return <div />
		}

		return (
			<CheckInWithLocationChange
				candidates={state.checkInOutCandidates}
				fadeIn={state.showUnfilteredSearch}
				onSearch={submitSecurityCodeWithoutLocationFilter}
				onLocationGroupChange={onChangeLocationGroupChange}
				onLocationChange={onChangeLocationChange}
				onSelectCandidate={selectCandidateForLocationChange}
				locationGroups={state.checkInOutLocationGroups}
				locations={state.locations['options'] ?? []}
				selectedLocation={state.selectedChangeLocation}
				onCheckIn={changeLocationAndCheckIn}
			/>
		)
	}

	const options = renderOptions()
	const input = renderInput()
	const alert = renderAlert()

	return (
		<Grid
			container
			spacing={3}
			justifyContent="space-between"
			alignItems="center"
		>
			<Grid item xs={12}>
				<h1 id="title">{state.checkType}</h1>
			</Grid>
			{options}
			{input}
			{alert}
			{state.showUnfilteredSearch ? <div /> : renderCandidates()}
			{state.showUnfilteredSearch ? <div /> : renderAdults()}
			{state.showUnfilteredSearch ? renderUnfilteredSearch() : <div />}
		</Grid>
	)
}

export const CheckInOut = withAuth(CheckIn)
