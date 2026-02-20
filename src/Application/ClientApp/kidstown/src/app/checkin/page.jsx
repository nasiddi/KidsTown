/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useState } from 'react'
import Grid from '@mui/material/Grid'
import { getSelectedOptionsFromStorage, getStateFromLocalStorage } from '../components/Common'
import { CheckInOptions } from './CheckInOptions'
import { CheckInInput } from './CheckInInput'
import { KidsTownAlert } from '../components/KidsTownAlert'
import { CheckInPhoneNumbers } from './CheckInPhoneNumbers'
import { CheckInCandidates } from './CheckInCandidates'
import { CheckInWithLocationChange } from './CheckInWithLocationChange'
import {
  fetchLocationGroups,
  fetchLocations,
  postCheckInOut,
  postSecurityCode,
  postWithJsonResult,
} from '../helpers/BackendClient'
import { NarrowLayout } from '../components/Layout'
import dayjs from 'dayjs'
import withAuth from '../components/withAuth'

const cleanState = {
  checkInOutCandidates: [],
  securityCode: '',
  showUnfilteredSearch: false,
  locations: [],
  allLocations: [],
  selectedChangeLocation: {},
  selectedChangeLocationGroupId: '',
}

const clearedAttendanceIds = { lastActionAttendanceIds: [] }

const clearedAlert = { alert: { text: '', level: 1 } }

function CheckIn() {
  const [state, setState] = useState({
    locationGroups: [],
    checkInOutCandidates: [],
    checkInOutLocationGroups: getSelectedOptionsFromStorage('checkInOutLocationGroups', []),
    securityCode: '',
    fastCheckInOut: getStateFromLocalStorage('fastCheckInOut', true),
    singleCheckInOut: getStateFromLocalStorage('singleCheckInOut', false),
    showPhoneNumbers: getStateFromLocalStorage('showPhoneNumbers', true),
    alert: { text: '', level: 1 },
    checkType: localStorage.getItem('checkType') ?? 'CheckIn',
    loading: true,
    lastActionAttendanceIds: [],
    lastCodeSubmission: dayjs().startOf('date'),
    showUnfilteredSearch: false,
    availableLocations: [],
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
      const allLocations = await fetchLocations()
      setState({
        ...state,
        locationGroups: locationGroups,
        allLocations: allLocations,
        loading: false,
      })
    }

    load().then()
  }, [])

  useEffect(() => {
    if (state.securityCode.length === 4) {
      const currentDate = new Date()
      if (currentDate - state.lastCodeSubmission > 2000) {
        submitSecurityCodeWithLocationFilter().then()
      } else {
        setState({ ...state, ...cleanState })
      }
    }
  }, [state.securityCode])

  const updateOptions = (options, key) => {
    localStorage.setItem(key.name, JSON.stringify(options))
    setState({
      ...state,
      ...cleanState,
      ...clearedAlert,
      ...clearedAttendanceIds,
      [key.name]: [...options],
    })
  }

  function onLocationDeselect(event) {
    let id = event.target.parentElement.id
    if (id.length === 0) {
      id = event.target.parentElement.parentElement.id
    }

    const options = state.checkInOutLocationGroups.filter((l) => l.value.toString() !== id)

    updateOptions(options, { name: 'checkInOutLocationGroups' })
  }

  function onLocationSelect(event) {
    const location = state.locationGroups.find((l) => l.value.toString() === event.target.id)
    const options = state.checkInOutLocationGroups
    options.push(location)
    updateOptions(options, { name: 'checkInOutLocationGroups' })
  }

  async function onChangeLocationChange(event) {
    setState({
      ...state,
      selectedChangeLocation: state.availableLocations.options.find(
        (l) => l.value === event.target.value,
      ),
    })
  }

  async function onChangeLocationGroupChange(event) {
    const locationsByGroup = state.allLocations.find(
      (l) => l.groupId === parseInt(event.target.value, 10),
    )

    setState({
      ...state,
      availableLocations: locationsByGroup,
      selectedChangeLocationGroupId: locationsByGroup.groupId,
      selectedChangeLocation:
        locationsByGroup.options.length === 1 ? locationsByGroup['options'][0] : {},
    })
  }

  const selectCandidateForLocationChange = (event) => {
    const candidate = state.checkInOutCandidates.find(
      (c) => c['attendanceId'] === parseInt(event.target.id, 10),
    )
    setState({ ...state, checkInOutCandidates: [candidate] })
  }

  const handleChange = (event) => {
    localStorage.setItem(event.target.name, event.target.checked)
    setState({ ...state, [event.target.name]: event.target.checked })
  }

  const updateSecurityCode = (e) => {
    setState({ ...state, securityCode: e.target.value })
  }

  async function submitSecurityCodeWithoutLocationFilter() {
    const json = await postSecurityCode(
      state.securityCode,
      [],
      state.fastCheckInOut,
      state.checkType,
      false,
    )

    setState({
      ...state,
      alert: { level: json['alertLevel'], text: json['text'] },
      lastActionAttendanceIds: [],
      checkInOutCandidates: json['checkInOutCandidates'],
    })
  }

  async function submitSecurityCodeWithLocationFilter() {
    const selectedLocationGroupIds = state.checkInOutLocationGroups.map((l) => l.value)
    const json = await postSecurityCode(
      state.securityCode,
      selectedLocationGroupIds,
      state.fastCheckInOut,
      state.checkType,
      true,
    )

    const attendanceIds = json['attendanceIds'] ?? []

    const alert = {
      level: json['alertLevel'],
      text: json['text'],
    }

    const candidates = json['checkInOutCandidates'].map(function (el) {
      const o = Object.assign({}, el)
      o.isSelected = true

      return o
    })

    if (
      json['successfulFastCheckout'] === true ||
      (candidates.length === 0 && !json['filteredSearchUnsuccessful'])
    ) {
      const newVar = {
        ...state,
        ...cleanState,
        lastActionAttendanceIds: attendanceIds,
        lastCodeSubmission: new Date(),
        alert: alert,
      }
      setState(newVar)
    } else {
      setState({
        ...state,
        alert: alert,
        checkInOutCandidates: candidates,
        lastCodeSubmission: new Date(),
        showUnfilteredSearch: json['filteredSearchUnsuccessful'],
        lastActionAttendanceIds: json['filteredSearchUnsuccessful'] ? [] : attendanceIds,
      })
    }
  }

  async function checkInOutMultiple() {
    const candidates = state.checkInOutCandidates.filter((c) => c.isSelected)
    const json = await postCheckInOut(candidates, state.checkType, state.securityCode)
    await processCheckInOutResult(json)
  }

  async function checkInOutSingle(event) {
    const candidate = state.checkInOutCandidates.find(
      (c) => c['attendanceId'] === parseInt(event.target.id, 10),
    )
    const json = await postCheckInOut([candidate], state.checkType, state.securityCode)
    await processCheckInOutResult(json)
  }

  async function changeLocationAndCheckIn() {
    const candidate = state.checkInOutCandidates[0]
    candidate.locationId = state.selectedChangeLocation.value
    const json = await postWithJsonResult('checkinout/manual-with-location-update', candidate)
    await processCheckInOutResult(json)
  }

  async function undoAction() {
    const json = await postWithJsonResult(
      `checkinout/undo/${state.checkType}`,
      state.lastActionAttendanceIds,
    )

    setState({
      ...state,
      ...cleanState,
      ...clearedAttendanceIds,
      alert: { level: json['alertLevel'], text: json['text'] },
      lastActionAttendanceIds: [],
    })
  }

  async function processCheckInOutResult(json) {
    const attendanceIds = json['attendanceIds'] ?? []

    setState({
      ...state,
      ...cleanState,
      alert: { level: json['alertLevel'], text: json['text'] },
      lastActionAttendanceIds: attendanceIds,
    })
  }

  function resetView() {
    setState({
      ...state,
      ...cleanState,
      ...clearedAlert,
      ...clearedAttendanceIds,
    })
  }

  function invertSelectCandidate(event) {
    const candidate = state.checkInOutCandidates.find(
      (c) => c['attendanceId'] === parseInt(event.target.id, 10),
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
        ...clearedAttendanceIds,
        checkType: event.target.id,
      })
      localStorage.setItem('checkType', event.target.id)
    }
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
        locationGroups={state.locationGroups}
        checkInOutLocationGroups={state.checkInOutLocationGroups}
        onLocationDeselect={onLocationDeselect}
        onLocationSelect={onLocationSelect}
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
      <KidsTownAlert
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

  function renderPhoneNumbers() {
    return state.checkType !== 'CheckIn' || !state.showPhoneNumbers ? (
      <div />
    ) : (
      <Grid size={12}>
        <CheckInPhoneNumbers attendanceIds={state.lastActionAttendanceIds} />
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
        locations={state.availableLocations['options'] ?? []}
        selectedLocation={state.selectedChangeLocation}
        selectedLocationGroupId={state.selectedChangeLocationGroupId}
        onCheckIn={changeLocationAndCheckIn}
      />
    )
  }

  const options = renderOptions()
  const input = renderInput()
  const alert = renderAlert()

  return (
    <NarrowLayout>
      <Grid container spacing={3} justifyContent="space-between" alignItems="center">
        <Grid size={12}>
          <h1 id="title">{state.checkType}</h1>
        </Grid>
        {options}
        {input}
        {alert}
        {state.showUnfilteredSearch ? <div /> : renderCandidates()}
        {state.showUnfilteredSearch ? <div /> : renderPhoneNumbers()}
        {state.showUnfilteredSearch ? renderUnfilteredSearch() : <div />}
      </Grid>
    </NarrowLayout>
  )
}

export default withAuth(CheckIn)
