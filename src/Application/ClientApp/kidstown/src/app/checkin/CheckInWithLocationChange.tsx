import { FormControl, InputLabel, MenuItem, Select } from '@mui/material'
import type { SelectChangeEvent } from '@mui/material'
import Grid from '@mui/material/Grid'
import type React from 'react'

import { LargeButton } from '../components/Common'
import type {
  CheckInOutCandidate,
  LocationGroupOption,
  LocationOption,
} from '../helpers/BackendClient'

interface CheckInWithLocationChangeProps {
  candidates: CheckInOutCandidate[]
  fadeIn: boolean
  onSearch: React.MouseEventHandler<HTMLButtonElement>
  onLocationGroupChange: (event: SelectChangeEvent) => void
  onLocationChange: (event: SelectChangeEvent) => void
  onSelectCandidate: React.MouseEventHandler<HTMLButtonElement>
  locationGroups: LocationGroupOption[]
  locations: LocationOption[]
  selectedLocation: Partial<LocationOption>
  selectedLocationGroupId: string | number
  onCheckIn: React.MouseEventHandler<HTMLButtonElement>
}

export function CheckInWithLocationChange({
  candidates,
  onSearch,
  onLocationGroupChange,
  onLocationChange,
  onSelectCandidate,
  locationGroups,
  locations,
  selectedLocation,
  selectedLocationGroupId,
  onCheckIn,
}: CheckInWithLocationChangeProps) {
  if (candidates.length === 0) {
    return (
      <Grid size={12}>
        <LargeButton
          id={'unfilteredSearch'}
          name={'Suche ohne Location Filter'}
          color={'warning'}
          onClick={onSearch}
        />
      </Grid>
    )
  }

  const candidateButtons = candidates.map((c) => (
    <Grid size={12} key={c.attendanceId}>
      <LargeButton id={c.attendanceId} name={c.name} color="primary" onClick={onSelectCandidate} />
    </Grid>
  ))

  if (candidates.length === 1) {
    return (
      <Grid size={12}>
        <Grid container spacing={2} justifyContent="space-between" alignItems="center">
          {candidateButtons}
          <Grid size={12}>
            <FormControl fullWidth>
              <InputLabel>Select Location</InputLabel>
              <Select
                name={'changeLocationGroup'}
                onChange={onLocationGroupChange}
                label={'Select Location'}
                value={selectedLocationGroupId?.toString() ?? ''}
              >
                {locationGroups.map((l) => (
                  <MenuItem value={l.value} key={l.value}>
                    {l.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          {locations.length > 1 ? (
            <Grid size={12}>
              <FormControl fullWidth>
                <InputLabel>Select SubLocation</InputLabel>
                <Select
                  name={'changeLocation'}
                  onChange={onLocationChange}
                  label={'Select SubLocation'}
                  value={selectedLocation.value?.toString() ?? ''}
                >
                  {locations.map((l) => (
                    <MenuItem value={l.value} key={l.value}>
                      {l.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          ) : (
            <div />
          )}
          {selectedLocation.value !== undefined ? (
            <Grid size={12}>
              <LargeButton
                id={'checkinWithLocationChange'}
                name={`CheckIn ${candidates[0].name} \u{2192} ${selectedLocation.label}`}
                color={'success'}
                onClick={onCheckIn}
              />
            </Grid>
          ) : (
            <div />
          )}
        </Grid>
      </Grid>
    )
  }

  return (
    <Grid size={12}>
      <Grid container spacing={3} justifyContent="space-between" alignItems="center">
        {candidateButtons}
      </Grid>
    </Grid>
  )
}
