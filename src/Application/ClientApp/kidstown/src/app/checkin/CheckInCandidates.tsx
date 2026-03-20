import Grid from '@mui/material/Grid'
import type React from 'react'

import { LargeButton } from '../components/Common'
import type { CheckInOutCandidate } from '../helpers/BackendClient'

interface CheckInCandidatesProps {
  candidates: CheckInOutCandidate[]
  isSingleCheckInOut: boolean
  invertSelectCandidate: React.MouseEventHandler<HTMLButtonElement>
  checkType: string
  onCheckInOutMultiple: React.MouseEventHandler<HTMLButtonElement>
  onCheckInOutSingle: React.MouseEventHandler<HTMLButtonElement>
}

export function CheckInCandidates({
  candidates,
  isSingleCheckInOut,
  invertSelectCandidate,
  checkType,
  onCheckInOutMultiple,
  onCheckInOutSingle,
}: CheckInCandidatesProps) {
  function renderMultiCheckout() {
    const candidateButtons = candidates.map((c) => (
      <Grid size={12} key={c.attendanceId}>
        <LargeButton
          id={c.attendanceId}
          name={c.name}
          color={getNameButtonColor(c)}
          onClick={invertSelectCandidate}
          isOutline={!c.isSelected}
        />
      </Grid>
    ))

    return (
      <Grid size={12}>
        <Grid container spacing={3} justifyContent="space-between" alignItems="center">
          {candidateButtons}
          <Grid size={12}>
            <LargeButton
              id={'submit'}
              name={checkType}
              color={areNoCandidatesSelected() ? 'secondary' : 'success'}
              onClick={onCheckInOutMultiple}
              isOutline={false}
              disabled={areNoCandidatesSelected()}
            />
          </Grid>
        </Grid>
      </Grid>
    )
  }

  function renderSingleCheckout() {
    const candidateButtons = candidates.map((c) => (
      <Grid size={12} key={c.attendanceId}>
        <LargeButton
          id={c.attendanceId}
          name={c.name}
          color={getNameButtonColor(c)}
          onClick={onCheckInOutSingle}
        />
      </Grid>
    ))

    return (
      <Grid size={12}>
        <Grid container spacing={3} justifyContent="space-between" alignItems="center">
          {candidateButtons}
        </Grid>
      </Grid>
    )
  }

  function getNameButtonColor(candidate: CheckInOutCandidate): 'primary' | 'error' | 'warning' {
    if (checkType === 'CheckIn') {
      return 'primary'
    }

    if (candidate.hasPeopleWithoutPickupPermission) {
      return 'error'
    }

    if (!candidate.mayLeaveAlone) {
      return 'warning'
    }

    return 'primary'
  }

  function areNoCandidatesSelected(): boolean {
    return candidates.filter((c) => c.isSelected).length <= 0
  }

  if (isSingleCheckInOut) {
    return renderSingleCheckout()
  }

  return renderMultiCheckout()
}
