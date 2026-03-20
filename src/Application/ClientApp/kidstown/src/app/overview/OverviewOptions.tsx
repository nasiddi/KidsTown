import { Alert, Box, CircularProgress } from '@mui/material'
import Grid from '@mui/material/Grid'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import dayjs from 'dayjs'
import { useEffect, useState } from 'react'
import type React from 'react'

import {
  getLastSunday,
  getSelectedOptionsFromStorage,
  getStringFromSession,
} from '../components/CommonHelpers'
import MultiSelect from '../components/MultiSelect'
import { onDeselect, onSelect } from '../components/MultiSelectHelpers'
import type { LocationGroupOption } from '../helpers/BackendClient'
import { fetchLocationGroups } from '../helpers/BackendClient'

const optionKey = 'overviewLocationGroups'

interface OverviewOptionsState {
  locationGroups: LocationGroupOption[]
  overviewLocationGroups: LocationGroupOption[]
  loading: boolean
  date: dayjs.Dayjs
  error: string | null
}

export default function OverviewOptions() {
  const [state, setState] = useState<OverviewOptionsState>({
    locationGroups: [],
    overviewLocationGroups: [],
    loading: true,
    date: dayjs(getLastSunday().toISOString().substring(0, 10)),
    error: null,
  })

  useEffect(() => {
    async function load() {
      try {
        const locationGroups = await fetchLocationGroups()
        const dateString = getStringFromSession(
          'overviewDate',
          getLastSunday().toISOString().substring(0, 10),
        )
        setState((prev) => ({
          ...prev,
          date: dayjs(dateString),
          locationGroups: locationGroups,
          loading: false,
          overviewLocationGroups: getSelectedOptionsFromStorage(
            'overviewLocationGroups',
            prev.overviewLocationGroups,
          ),
        }))
      } catch {
        setState((prev) => ({ ...prev, loading: false, error: 'Failed to load.' }))
      }
    }

    load().then()
  }, [])

  function updateDate(value: dayjs.Dayjs) {
    const overviewDate = `${value.year()}-${value.month() + 1}-${value.date()}`
    sessionStorage.setItem('overviewDate', overviewDate)
    setState({ ...state, date: value })
    window.dispatchEvent(new CustomEvent('overviewFiltersChanged'))
  }

  function onLocationDeselect(event: React.SyntheticEvent) {
    onDeselect(event, optionKey, state, setState as (s: unknown) => void)
    window.dispatchEvent(new CustomEvent('overviewFiltersChanged'))
  }

  function onLocationSelect(event: React.MouseEvent) {
    onSelect(event, optionKey, state.locationGroups, state, setState as (s: unknown) => void)
    window.dispatchEvent(new CustomEvent('overviewFiltersChanged'))
  }

  function renderOptions() {
    return (
      <div>
        <Grid container spacing={3} justifyContent="space-between" alignItems="center">
          <Grid size={9}>
            <MultiSelect
              options={state.locationGroups}
              selectedOptions={state.overviewLocationGroups}
              onSelectOption={onLocationSelect}
              onRemoveOption={onLocationDeselect}
            />
          </Grid>
          <Grid size={3}>
            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <DatePicker
                value={state.date}
                onChange={(newValue) => updateDate(newValue!)}
                format={'DD.MM.YYYY'}
              />
            </LocalizationProvider>
          </Grid>
        </Grid>
      </div>
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

  return (
    <div>
      <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>{renderOptions()}</Grid>
      </Grid>
    </div>
  )
}
