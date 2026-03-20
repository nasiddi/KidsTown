import { Divider } from '@mui/material'
import Grid from '@mui/material/Grid'
import type React from 'react'
import { useEffect, useState } from 'react'

import { StyledButton } from '../components/Common'
import { getSelectedFromSession } from '../components/CommonHelpers'

interface FilterButtonProps {
  color: 'primary' | 'secondary' | 'success' | 'error' | 'info' | 'warning'
  onClick: () => void
  isSelected: boolean
  label: string
}

function FilterButton({ color, onClick, isSelected, label }: FilterButtonProps) {
  return (
    <StyledButton
      color={color}
      onClick={onClick}
      variant={!isSelected ? 'outlined' : 'contained'}
      fullWidth={true}
      size={'large'}
    >
      {label}
    </StyledButton>
  )
}

type OverviewState = 'PreCheckedIn' | 'CheckedIn' | 'CheckedOut'

interface OverviewFilterState {
  selectedStates: OverviewState[]
}

export default function OverviewFilter() {
  const [state, setState] = useState<OverviewFilterState>({
    selectedStates: ['PreCheckedIn', 'CheckedIn', 'CheckedOut'],
  })

  useEffect(() => {
    setState({
      ...state,
      selectedStates: getSelectedFromSession('selectedOverviewStates', state.selectedStates),
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  function stateIsSelected(target: OverviewState): boolean {
    return state.selectedStates.includes(target)
  }

  function renderFilter() {
    return (
      <div>
        <Grid container spacing={3} justifyContent="space-between" alignItems="center">
          <Grid size={12}>
            <Divider sx={{ my: 1 }} />
          </Grid>
          <Grid size={4}>
            <FilterButton
              label="PreCheckedIn"
              color={'info'}
              onClick={togglePreCheckedIn}
              isSelected={stateIsSelected('PreCheckedIn')}
            />
          </Grid>
          <Grid size={4}>
            <FilterButton
              label="CheckedIn"
              color={'success'}
              onClick={toggleCheckedIn}
              isSelected={stateIsSelected('CheckedIn')}
            />
          </Grid>
          <Grid size={4}>
            <FilterButton
              label="CheckedOut"
              color="primary"
              onClick={toggleCheckedOut}
              isSelected={stateIsSelected('CheckedOut')}
            />
          </Grid>
        </Grid>
      </div>
    )
  }

  function togglePreCheckedIn() {
    toggleState('PreCheckedIn')
  }

  function toggleCheckedIn() {
    toggleState('CheckedIn')
  }

  function toggleCheckedOut() {
    toggleState('CheckedOut')
  }

  function toggleState(toggledState: OverviewState) {
    let currentStates = state.selectedStates
    if (currentStates.includes(toggledState)) {
      currentStates = currentStates.filter((p) => p !== toggledState)
    } else {
      currentStates.push(toggledState)
    }

    setState({ ...state, selectedStates: currentStates })
    sessionStorage.setItem('selectedOverviewStates', JSON.stringify(currentStates))
  }

  return (
    <div>
      <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>{renderFilter()}</Grid>
      </Grid>
    </div>
  )
}
