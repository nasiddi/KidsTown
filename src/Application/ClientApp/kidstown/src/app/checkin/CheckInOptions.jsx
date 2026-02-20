import React from 'react'
import Grid from '@mui/material/Grid'
import { PrimaryCheckBox, ToggleButtons } from '../components/Common'
import MultiSelect from '../components/MultiSelect'

export function CheckInOptions(props) {
  const toggleButtons = [
    {
      label: 'CheckIn',
      onClick: props.onClick,
      isSelected: props.checkType === 'CheckIn',
    },
    {
      label: 'CheckOut',
      onClick: props.onClick,
      isSelected: props.checkType === 'CheckOut',
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
            checked={props.fastCheckInOut}
            onChange={props.onCheckBoxChange}
            label={`Fast ${props.checkType}`}
          />
        </Grid>
        <Grid>
          <PrimaryCheckBox
            name="singleCheckInOut"
            checked={props.singleCheckInOut}
            onChange={props.onCheckBoxChange}
            label={`Single ${props.checkType}`}
          />
        </Grid>
        <Grid>
          <PrimaryCheckBox
            name="showPhoneNumbers"
            checked={props.showPhoneNumbers}
            onChange={props.onCheckBoxChange}
            label={'Show Numbers'}
            disabled={props.checkType !== 'CheckIn'}
          />
        </Grid>
        <Grid size={{ lg: 4, xs: 12 }} style={{ minWidth: '190px' }}>
          <MultiSelect
            options={props.locationGroups}
            selectedOptions={props.checkInOutLocationGroups}
            onSelectOption={props.onLocationSelect}
            onRemoveOption={props.onLocationDeselect}
          />
        </Grid>
      </Grid>
    </Grid>
  )
}
