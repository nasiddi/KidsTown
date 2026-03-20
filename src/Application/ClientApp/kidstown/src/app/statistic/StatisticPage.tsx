import {
  Alert,
  Box,
  CircularProgress,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import Grid from '@mui/material/Grid'
import dayjs from 'dayjs'
import { useEffect, useState } from 'react'
import type React from 'react'

import { LargeButton, TableHeadCell } from '../components/Common'
import { getFormattedDate, getSelectedOptionsFromStorage } from '../components/CommonHelpers'
import { NarrowLayout } from '../components/Layout'
import MultiSelect from '../components/MultiSelect'
import { getOnDeselectId } from '../components/MultiSelectHelpers'
import withAuth from '../components/withAuth'
import type {
  LocationGroupOption,
  LocationOption,
  LocationsByGroup,
  StatisticRow,
} from '../helpers/BackendClient'
import {
  fetchLocationGroups,
  fetchLocations,
  getSelectedEventFromStorage,
  post,
} from '../helpers/BackendClient'

interface StatisticState {
  locationGroups: LocationGroupOption[]
  allLocationGroups: LocationGroupOption[]
  allLocations: LocationsByGroup[]
  selectedLocations: LocationsByGroup[]
  attendees: StatisticRow[]
  renderLocationSelect: boolean
  loading: boolean
  loadingData: boolean
  furthestYear: number
  error: string | null
}

function Statistic() {
  const [state, setState] = useState<StatisticState>({
    locationGroups: [],
    allLocationGroups: [],
    allLocations: [],
    selectedLocations: [],
    attendees: [],
    renderLocationSelect: false,
    loading: true,
    loadingData: true,
    furthestYear: dayjs().year(),
    error: null,
  })

  useEffect(() => {
    async function load() {
      try {
        const allLocationGroups = await fetchLocationGroups()
        const allLocations = await fetchLocations()
        const selectedLocations = getSelectedOptionsFromStorage<LocationsByGroup[]>(
          'statisticLocations',
          [],
        )
        setState((prev) => ({
          ...prev,
          allLocationGroups,
          allLocations,
          selectedLocations,
          loading: false,
        }))
      } catch {
        setState((prev) => ({ ...prev, loading: false, error: 'Failed to load.' }))
      }
    }

    load().then()
  }, [])

  useEffect(() => {
    async function fetchData(locations: LocationsByGroup[]): Promise<StatisticRow[]> {
      if (locations.length === 0) {
        return []
      }

      return await post(
        `overview/event/${await getSelectedEventFromStorage()}/attendees/history?startDate=${
          state.furthestYear
        }-01-01&endDate=${dayjs().year()}-12-31`,
        locations.flatMap((l) =>
          l.options.filter((o: LocationOption) => o.isSelected).map((o: LocationOption) => o.value),
        ),
      )
        .then((r) => r.json())
        .then((j: StatisticRow[]) => {
          return j
        })
    }

    async function load() {
      try {
        const attendees = await fetchData(state.selectedLocations)
        setState((prev) => ({
          ...prev,
          attendees: attendees,
          loadingData: false,
        }))
      } catch {
        setState((prev) => ({ ...prev, loadingData: false, error: 'Failed to load.' }))
      }
    }

    load().then()
  }, [state.selectedLocations, state.furthestYear])

  function updateLocalStorage(locations: LocationsByGroup[]) {
    localStorage.setItem('statisticLocations', JSON.stringify(locations))
  }

  function onLocationGroupSelect(event: React.MouseEvent) {
    const target = event.target as HTMLElement
    const locations = state.allLocations.find((l) => l.groupId === parseInt(target.id, 10))

    if (!locations) return

    const selectedLocations = [...state.selectedLocations, locations]
    updateLocalStorage(selectedLocations)

    setState({
      ...state,
      selectedLocations: selectedLocations,
    })
  }

  function onLocationGroupDeselect(event: React.SyntheticEvent) {
    const id = getOnDeselectId(event)

    const selectedLocations = state.selectedLocations.filter(
      (l) => l.groupId !== parseInt(String(id), 10),
    )

    updateLocalStorage(selectedLocations)

    setState({
      ...state,
      selectedLocations: selectedLocations,
    })
  }

  function updateOption(id: number, isSelected: boolean) {
    const locations = state.selectedLocations.find((l) => {
      return l.options.some((o) => o.value === id)
    })

    if (locations) {
      const option = locations.options.find((o) => o.value === id)
      if (option) {
        option.isSelected = isSelected
      }
    }

    updateLocalStorage(state.selectedLocations)

    setState({
      ...state,
      selectedLocations: [...state.selectedLocations],
    })
  }

  function onLocationSelect(event: React.MouseEvent) {
    const target = event.target as HTMLElement
    const id = parseInt(target.id, 10)
    updateOption(id, true)
  }

  function onLocationDeselect(event: React.SyntheticEvent) {
    const id = getOnDeselectId(event)
    updateOption(id, false)
  }

  function loadAnotherYear() {
    setState({
      ...state,
      furthestYear: state.furthestYear - 1,
      loadingData: true,
    })
  }

  function renderLocationGroupSelect() {
    const selectedGroups = state.allLocationGroups.filter((g) =>
      state.selectedLocations.map((l) => l.groupId).includes(g.value),
    )

    return (
      <div>
        <Grid container spacing={3} justifyContent="space-between" alignItems="center">
          <Grid size={12}>
            <MultiSelect
              options={state.allLocationGroups}
              selectedOptions={selectedGroups}
              onSelectOption={onLocationGroupSelect}
              onRemoveOption={onLocationGroupDeselect}
            />
          </Grid>
        </Grid>
      </div>
    )
  }

  function renderLocationSelect() {
    return (
      <div>
        <Grid container spacing={3} justifyContent="space-between" alignItems="center">
          <Grid size={12}>
            <MultiSelect
              options={state.selectedLocations
                .filter((l) => l.options.length > 1)
                .flatMap((l) => l.options.filter((o) => !o.isSelected))}
              selectedOptions={state.selectedLocations
                .filter((l) => l.options.length > 1)
                .flatMap((l) => l.options.filter((o) => o.isSelected))}
              onSelectOption={onLocationSelect}
              onRemoveOption={onLocationDeselect}
            />
          </Grid>
        </Grid>
      </div>
    )
  }

  function renderCounts() {
    if (state.loading) {
      return (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      )
    }

    return (
      <Table size={'small'}>
        <TableHead>
          <TableRow>
            <TableHeadCell>
              <Typography component="span" variant="body2" fontWeight="bold">
                Datum
              </Typography>
            </TableHeadCell>
            <TableHeadCell align="right">
              <Typography component="span" variant="body2" fontWeight="bold">
                Kinder
              </Typography>
            </TableHeadCell>
            <TableHeadCell align="right">
              <Typography component="span" variant="body2" fontWeight="bold">
                davon G&auml;ste
              </Typography>
            </TableHeadCell>
            <TableHeadCell align="right">
              <Typography component="span" variant="body2" fontWeight="bold">
                Betreuer
              </Typography>
            </TableHeadCell>
            <TableHeadCell align="right">
              <Typography component="span" variant="body2" fontWeight="bold">
                kein CheckIn
              </Typography>
            </TableHeadCell>
            <TableHeadCell align="right">
              <Typography component="span" variant="body2" fontWeight="bold">
                kein CheckOut
              </Typography>
            </TableHeadCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {state.attendees
            .sort((a, b) => (a.date > b.date ? -1 : 1))
            .map((row) => (
              <TableRow key={row.date}>
                <TableCell>{getFormattedDate(row.date)}</TableCell>
                <TableCell align="right">{row.regularCount + row.guestCount}</TableCell>
                <TableCell align="right">{row.guestCount}</TableCell>
                <TableCell align="right">{row.volunteerCount}</TableCell>
                <TableCell align="right">{row.preCheckInOnlyCount}</TableCell>
                <TableCell align="right">{row.noCheckOutCount}</TableCell>
              </TableRow>
            ))}
        </TableBody>
      </Table>
    )
  }

  if (state.loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (state.error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        {state.error}
      </Alert>
    )
  }

  const locationSelect = state.selectedLocations.some((s) => s.options.length > 1) ? (
    renderLocationSelect()
  ) : (
    <div />
  )

  const noMoreDataAvailable = !state.attendees.some((a) =>
    a.date.startsWith(String(state.furthestYear)),
  )

  return (
    <NarrowLayout>
      <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>{renderLocationGroupSelect()}</Grid>
        <Grid size={12}>{locationSelect}</Grid>
        <Grid size={12}>{renderCounts()}</Grid>
        <Grid size={12}>
          <LargeButton
            name={
              state.loadingData
                ? 'loading'
                : noMoreDataAvailable
                  ? 'No more data'
                  : `Load ${state.furthestYear - 1}`
            }
            onClick={loadAnotherYear}
            disabled={noMoreDataAvailable}
          />
        </Grid>
      </Grid>
      <Divider sx={{ my: 1 }} />
    </NarrowLayout>
  )
}

const StatisticPage = withAuth(Statistic)
export default StatisticPage
