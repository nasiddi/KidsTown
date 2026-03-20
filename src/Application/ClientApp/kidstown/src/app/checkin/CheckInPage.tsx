/* eslint-disable react-hooks/exhaustive-deps */

import type { SelectChangeEvent } from '@mui/material'
import { Alert, Box, CircularProgress, Typography } from '@mui/material'
import Grid from '@mui/material/Grid'
import dayjs from 'dayjs'
import { useEffect, useRef, useState } from 'react'
import type React from 'react'

import {
  getSelectedOptionsFromStorage,
  getStateFromLocalStorage,
} from '../components/CommonHelpers'
import { KidsTownAlert } from '../components/KidsTownAlert'
import { NarrowLayout } from '../components/Layout'
import withAuth from '../components/withAuth'
import type {
  AlertInfo,
  CheckInOutCandidate,
  CheckInOutResponse,
  LocationGroupOption,
  LocationOption,
  LocationsByGroup,
} from '../helpers/BackendClient'
import {
  fetchLocationGroups,
  fetchLocations,
  postCheckInOut,
  postSecurityCode,
  postWithJsonResult,
} from '../helpers/BackendClient'

import { CheckInCandidates } from './CheckInCandidates'
import { CheckInInput } from './CheckInInput'
import { CheckInOptions } from './CheckInOptions'
import { CheckInPhoneNumbers } from './CheckInPhoneNumbers'
import { CheckInWithLocationChange } from './CheckInWithLocationChange'

interface CheckInState {
  locationGroups: LocationGroupOption[]
  checkInOutCandidates: CheckInOutCandidate[]
  checkInOutLocationGroups: LocationGroupOption[]
  securityCode: string
  fastCheckInOut: boolean
  singleCheckInOut: boolean
  showPhoneNumbers: boolean
  alert: AlertInfo
  checkType: string
  loading: boolean
  error: string | null
  lastActionAttendanceIds: number[]
  lastCodeSubmission: Date | dayjs.Dayjs
  showUnfilteredSearch: boolean
  availableLocations: Partial<LocationsByGroup>
  allLocations: LocationsByGroup[]
  selectedChangeLocation: Partial<LocationOption>
  selectedChangeLocationGroupId: string | number
}

const cleanState: Partial<CheckInState> = {
  checkInOutCandidates: [],
  securityCode: '',
  showUnfilteredSearch: false,
  allLocations: [],
  selectedChangeLocation: {},
  selectedChangeLocationGroupId: '',
}

const clearedAttendanceIds: Partial<CheckInState> = { lastActionAttendanceIds: [] }

const clearedAlert: Partial<CheckInState> = { alert: { text: '', level: 1 } }

function CheckIn() {
  const [state, setState] = useState<CheckInState>({
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
    error: null,
    lastActionAttendanceIds: [],
    lastCodeSubmission: dayjs().startOf('date'),
    showUnfilteredSearch: false,
    availableLocations: {},
    allLocations: [],
    selectedChangeLocation: {},
    selectedChangeLocationGroupId: '',
  })

  const securityCodeInput = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (securityCodeInput.current) {
      securityCodeInput.current.focus()
    }
  }, [state])

  useEffect(() => {
    async function load() {
      try {
        const locationGroups = await fetchLocationGroups()
        const allLocations = await fetchLocations()
        setState((prev) => ({
          ...prev,
          locationGroups: locationGroups,
          allLocations: allLocations,
          loading: false,
        }))
      } catch {
        setState((prev) => ({ ...prev, loading: false, error: 'Failed to load.' }))
      }
    }

    load().then()
  }, [])

  useEffect(() => {
    if (state.securityCode.length === 4) {
      const currentDate = new Date()
      if (currentDate.getTime() - new Date(state.lastCodeSubmission as Date).getTime() > 2000) {
        submitSecurityCodeWithLocationFilter().then()
      } else {
        setState({ ...state, ...cleanState } as CheckInState)
      }
    }
  }, [state.securityCode])

  const updateOptions = (options: LocationGroupOption[], key: { name: string }) => {
    localStorage.setItem(key.name, JSON.stringify(options))
    setState({
      ...state,
      ...cleanState,
      ...clearedAlert,
      ...clearedAttendanceIds,
      [key.name]: [...options],
    } as CheckInState)
  }

  function onLocationDeselect(event: React.SyntheticEvent) {
    const target = event.target as HTMLElement
    let id = target.parentElement!.id
    if (id.length === 0) {
      id = target.parentElement!.parentElement!.id
    }

    const options = state.checkInOutLocationGroups.filter((l) => l.value.toString() !== id)

    updateOptions(options, { name: 'checkInOutLocationGroups' })
  }

  function onLocationSelect(event: React.MouseEvent) {
    const target = event.target as HTMLElement
    const location = state.locationGroups.find((l) => l.value.toString() === target.id)
    const options = state.checkInOutLocationGroups
    if (location) {
      options.push(location)
    }
    updateOptions(options, { name: 'checkInOutLocationGroups' })
  }

  async function onChangeLocationChange(event: SelectChangeEvent) {
    setState({
      ...state,
      selectedChangeLocation:
        state.availableLocations.options?.find((l) => l.value === Number(event.target.value)) ?? {},
    })
  }

  async function onChangeLocationGroupChange(event: SelectChangeEvent) {
    const locationsByGroup = state.allLocations.find(
      (l) => l.groupId === parseInt(event.target.value, 10),
    )!

    setState({
      ...state,
      availableLocations: locationsByGroup,
      selectedChangeLocationGroupId: locationsByGroup.groupId,
      selectedChangeLocation:
        locationsByGroup.options.length === 1 ? locationsByGroup.options[0] : {},
    })
  }

  const selectCandidateForLocationChange = (event: React.MouseEvent<HTMLButtonElement>) => {
    const target = event.target as HTMLElement
    const candidate = state.checkInOutCandidates.find(
      (c) => c.attendanceId === parseInt(target.id, 10),
    )
    setState({ ...state, checkInOutCandidates: candidate ? [candidate] : [] })
  }

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    localStorage.setItem(event.target.name, String(event.target.checked))
    setState({ ...state, [event.target.name]: event.target.checked } as CheckInState)
  }

  const updateSecurityCode = (e: React.ChangeEvent<HTMLInputElement>) => {
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
      alert: { level: json.alertLevel, text: json.text },
      lastActionAttendanceIds: [],
      checkInOutCandidates: json.checkInOutCandidates,
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

    const attendanceIds = json.attendanceIds ?? []

    const alert: AlertInfo = {
      level: json.alertLevel,
      text: json.text,
    }

    const candidates = json.checkInOutCandidates.map(function (el) {
      const o = Object.assign({}, el)
      o.isSelected = true

      return o
    })

    if (
      json.successfulFastCheckout === true ||
      (candidates.length === 0 && !json.filteredSearchUnsuccessful)
    ) {
      const newVar = {
        ...state,
        ...cleanState,
        lastActionAttendanceIds: attendanceIds,
        lastCodeSubmission: new Date(),
        alert: alert,
      }
      setState(newVar as CheckInState)
    } else {
      setState({
        ...state,
        alert: alert,
        checkInOutCandidates: candidates,
        lastCodeSubmission: new Date(),
        showUnfilteredSearch: json.filteredSearchUnsuccessful ?? false,
        lastActionAttendanceIds: json.filteredSearchUnsuccessful ? [] : attendanceIds,
      })
    }
  }

  async function checkInOutMultiple() {
    const candidates = state.checkInOutCandidates.filter((c) => c.isSelected)
    const json = await postCheckInOut(candidates, state.checkType, state.securityCode)
    await processCheckInOutResult(json)
  }

  async function checkInOutSingle(event: React.MouseEvent<HTMLButtonElement>) {
    const target = event.target as HTMLElement
    const candidate = state.checkInOutCandidates.find(
      (c) => c.attendanceId === parseInt(target.id, 10),
    )!
    const json = await postCheckInOut([candidate], state.checkType, state.securityCode)
    await processCheckInOutResult(json)
  }

  async function changeLocationAndCheckIn() {
    const candidate = state.checkInOutCandidates[0]
    candidate.locationId = state.selectedChangeLocation.value
    const json: CheckInOutResponse = await postWithJsonResult(
      'checkinout/manual-with-location-update',
      candidate,
    )
    await processCheckInOutResult(json)
  }

  async function undoAction() {
    const json: CheckInOutResponse = await postWithJsonResult(
      `checkinout/undo/${state.checkType}`,
      state.lastActionAttendanceIds,
    )

    setState({
      ...state,
      ...cleanState,
      ...clearedAttendanceIds,
      alert: { level: json.alertLevel, text: json.text },
      lastActionAttendanceIds: [],
    } as CheckInState)
  }

  async function processCheckInOutResult(json: CheckInOutResponse) {
    const attendanceIds = json.attendanceIds ?? []

    setState({
      ...state,
      ...cleanState,
      alert: { level: json.alertLevel, text: json.text },
      lastActionAttendanceIds: attendanceIds,
    } as CheckInState)
  }

  function resetView() {
    setState({
      ...state,
      ...cleanState,
      ...clearedAlert,
      ...clearedAttendanceIds,
    } as CheckInState)
  }

  function invertSelectCandidate(event: React.MouseEvent<HTMLButtonElement>) {
    const target = event.target as HTMLElement
    const candidate = state.checkInOutCandidates.find(
      (c) => c.attendanceId === parseInt(target.id, 10),
    )!
    candidate.isSelected = !candidate.isSelected
    setState({ ...state, checkInOutCandidates: state.checkInOutCandidates })
  }

  function selectCheckType(event: React.MouseEvent<HTMLButtonElement>) {
    const target = event.target as HTMLElement
    if (state.checkType !== target.id) {
      setState({
        ...state,
        ...cleanState,
        ...clearedAlert,
        ...clearedAttendanceIds,
        checkType: target.id,
      } as CheckInState)
      localStorage.setItem('checkType', target.id)
    }
  }

  function renderOptions() {
    return state.loading ? (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
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
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
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
        locations={state.availableLocations.options ?? []}
        selectedLocation={state.selectedChangeLocation}
        selectedLocationGroupId={state.selectedChangeLocationGroupId}
        onCheckIn={changeLocationAndCheckIn}
      />
    )
  }

  const options = renderOptions()
  const input = renderInput()
  const alert = renderAlert()

  if (state.error) {
    return (
      <NarrowLayout>
        <Alert severity="error" sx={{ m: 2 }}>
          {state.error}
        </Alert>
      </NarrowLayout>
    )
  }

  return (
    <NarrowLayout>
      <Grid container spacing={3} justifyContent="space-between" alignItems="center">
        <Grid size={12}>
          <Typography variant="h5" component="h1" id="title">
            {state.checkType}
          </Typography>
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

const CheckInPage = withAuth(CheckIn)
export default CheckInPage
