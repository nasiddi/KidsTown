import Grid from '@mui/material/Grid'
import type React from 'react'

import { PrimaryCheckBox, ToggleButtons } from '../components/Common'
import MultiSelect from '../components/MultiSelect'
import type { LocationGroupOption } from '../helpers/BackendClient'

interface CheckInOptionsProps {
  onClick: React.MouseEventHandler<HTMLButtonElement>
  checkType: string
  fastCheckInOut: boolean
  singleCheckInOut: boolean
  showPhoneNumbers: boolean
  onCheckBoxChange: React.ChangeEventHandler<HTMLInputElement>
  locationGroups: LocationGroupOption[]
  checkInOutLocationGroups: LocationGroupOption[]
  onLocationDeselect: (event: React.SyntheticEvent) => void
  onLocationSelect: (event: React.MouseEvent<HTMLLIElement>) => void
}

export function CheckInOptions({
  onClick,
  checkType,
  fastCheckInOut,
  singleCheckInOut,
  showPhoneNumbers,
  onCheckBoxChange,
  locationGroups,
  checkInOutLocationGroups,
  onLocationDeselect,
  onLocationSelect,
}: CheckInOptionsProps) {
  const toggleButtons = [
    {
      label: 'CheckIn',
      onClick: onClick,
      isSelected: checkType === 'CheckIn',
    },
    {
      label: 'CheckOut',
      onClick: onClick,
      isSelected: checkType === 'CheckOut',
    },
  ]

  return (
    <Grid size={12}>
      <Grid container spacing={1} justifyContent="space-between" alignItems="center">
        <Grid size={{ md: 'auto', xs: 12 }}>
          <ToggleButtons buttons={toggleButtons} />
        </Grid>
        <Grid>
          <PrimaryCheckBox
            name="fastCheckInOut"
            checked={fastCheckInOut}
            onChange={onCheckBoxChange}
            label={`Fast ${checkType}`}
          />
        </Grid>
        <Grid>
          <PrimaryCheckBox
            name="singleCheckInOut"
            checked={singleCheckInOut}
            onChange={onCheckBoxChange}
            label={`Single ${checkType}`}
          />
        </Grid>
        <Grid>
          <PrimaryCheckBox
            name="showPhoneNumbers"
            checked={showPhoneNumbers}
            onChange={onCheckBoxChange}
            label={'Show Numbers'}
            disabled={checkType !== 'CheckIn'}
          />
        </Grid>
        <Grid size={{ lg: 4, xs: 12 }} style={{ minWidth: '190px' }}>
          <MultiSelect
            options={locationGroups}
            selectedOptions={checkInOutLocationGroups}
            onSelectOption={onLocationSelect}
            onRemoveOption={onLocationDeselect}
          />
        </Grid>
      </Grid>
    </Grid>
  )
}
