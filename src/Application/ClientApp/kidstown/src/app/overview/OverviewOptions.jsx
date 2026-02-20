import React, { useEffect, useState } from 'react'
import Grid from '@mui/material/Grid'
import {
  getLastSunday,
  getSelectedOptionsFromStorage,
  getStringFromSession,
} from '../components/Common'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import MultiSelect, { onDeselect, onSelect } from '../components/MultiSelect'
import dayjs from 'dayjs'
import { fetchLocationGroups } from '../helpers/BackendClient'

const optionKey = 'overviewLocationGroups'

export default function OverviewOptions() {
  const [state, setState] = useState({
    locationGroups: [],
    overviewLocationGroups: [],
    loading: true,
    date: dayjs(getLastSunday().toISOString().substring(0, 10)),
  })

  useEffect(() => {
    async function load() {
      const locationGroups = await fetchLocationGroups()
      const dateString = getStringFromSession(
        'overviewDate',
        getLastSunday().toISOString().substring(0, 10),
      )
      setState({
        ...state,
        date: dayjs(dateString),
        locationGroups: locationGroups,
        loading: false,
        overviewLocationGroups: getSelectedOptionsFromStorage(
          'overviewLocationGroups',
          state.overviewLocationGroups,
        ),
      })
    }

    load().then()
  }, [])

  function updateDate(value) {
    const overviewDate = `${value.$y}-${value.$M + 1}-${value.$D}`
    sessionStorage.setItem('overviewDate', overviewDate)
    setState({ ...state, date: value })
  }

  function onLocationDeselect(event) {
    onDeselect(event, optionKey, state, setState)
  }

  function onLocationSelect(event) {
    onSelect(event, optionKey, state.locationGroups, state, setState)
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
                onChange={(newValue) => updateDate(newValue)}
                format={'DD.MM.YYYY'}
              />
            </LocalizationProvider>
          </Grid>
        </Grid>
      </div>
    )
  }

  if (state.loading) {
    return <div />
  }

  return (
    <div>
      <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>{renderOptions()}</Grid>
      </Grid>
    </div>
  )
}
