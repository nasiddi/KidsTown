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
import { useEffect, useState } from 'react'

import { TableHeadCell, TableTotalCell } from '../components/Common'
import {
  getLastSunday,
  getSelectedOptionsFromStorage,
  getStringFromSession,
} from '../components/CommonHelpers'
import type { HeadCountRow, LocationGroupOption } from '../helpers/BackendClient'
import {
  fetchLocationGroups,
  getSelectedEventFromStorage,
  postWithJsonResult,
} from '../helpers/BackendClient'

interface HeadCountState {
  headCounts: HeadCountRow[]
  locationGroups: LocationGroupOption[]
  loading: boolean
  error: string | null
}

export default function OverviewHeadCount() {
  const [state, setState] = useState<HeadCountState>({
    headCounts: [],
    locationGroups: [],
    loading: true,
    error: null,
  })

  const sum = (a: number, b: number): number => {
    return a + b
  }

  async function fetchData(): Promise<HeadCountRow[]> {
    const selectedOptions = getSelectedOptionsFromStorage<LocationGroupOption[]>(
      'overviewLocationGroups',
      [],
    )

    if (selectedOptions.length === 0) {
      return []
    }

    return await postWithJsonResult(
      `overview/event/${await getSelectedEventFromStorage()}/attendees/headcounts?date=${getStringFromSession(
        'overviewDate',
        getLastSunday().toISOString(),
      )}`,
      selectedOptions.map((l) => l.value),
    )
  }

  async function loadData() {
    try {
      const locationGroups = await fetchLocationGroups()
      const headCounts = await fetchData()
      setState((prev) => ({
        ...prev,
        loading: false,
        locationGroups: locationGroups,
        headCounts: headCounts,
      }))
    } catch {
      setState((prev) => ({ ...prev, loading: false, error: 'Failed to load.' }))
    }
  }

  useEffect(() => {
    loadData().then()
    const interval = setInterval(() => {
      loadData().then()
    }, 5000)

    return () => clearInterval(interval)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  function renderCounts() {
    if (state.headCounts.length === 0) {
      return (
        <Typography color="text.secondary">
          No check-in data for the selected date and locations.
        </Typography>
      )
    }

    const headCounts = state.headCounts

    let totalCount = <TableRow />
    if (headCounts.length !== 1) {
      totalCount = (
        <TableRow key="Total">
          <TableTotalCell>
            <strong>Total</strong>
          </TableTotalCell>
          <TableTotalCell>
            <strong>{getTotalCount(false)}</strong>
          </TableTotalCell>
          <TableTotalCell>
            <strong>{getTotalCount(true)}</strong>
          </TableTotalCell>
        </TableRow>
      )
    }

    return (
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableHeadCell>
              <strong>Location</strong>
            </TableHeadCell>
            <TableHeadCell>
              <strong>Kinder</strong>
            </TableHeadCell>
            <TableHeadCell>
              <strong>Betreuer</strong>
            </TableHeadCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {headCounts.map((row) => (
            <TableRow key={row.location}>
              <TableCell> {row.location}</TableCell>
              <TableCell>{getCount(row, false)}</TableCell>
              <TableCell>{getCount(row, true)}</TableCell>
            </TableRow>
          ))}
          {totalCount}
        </TableBody>
      </Table>
    )
  }

  function getCount(headCounts: HeadCountRow, isVolunteer: boolean): number {
    if (isVolunteer) {
      return headCounts.volunteersCount
    }

    return headCounts.kidsCount
  }

  function getTotalCount(isVolunteer: boolean): number {
    if (isVolunteer) {
      const locationCount = state.headCounts.map((h) => h.volunteersCount)

      return locationCount.reduce(sum, 0)
    }

    const locationCount = state.headCounts.map((h) => h.kidsCount)

    return locationCount.reduce(sum, 0)
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

  return (
    <div>
      <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>
          <Divider sx={{ my: 1 }} />
        </Grid>
        <Grid size={12}>
          <Typography variant="subtitle1" component="h3" fontWeight="bold">
            Checked-In
          </Typography>
        </Grid>
        <Grid size={12}>{renderCounts()}</Grid>
      </Grid>
    </div>
  )
}
