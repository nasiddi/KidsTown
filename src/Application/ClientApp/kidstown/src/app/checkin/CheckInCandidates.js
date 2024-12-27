'use client'
import React from 'react'
import Grid from '@mui/material/Grid2';
import {LargeButton} from '../components/Common'

export function CheckInCandidates(props) {
    function renderMultiCheckout() {
        const candidates = props.candidates.map((c) => (
            <Grid size={12} key={c['attendanceId']}>
                <LargeButton
                    id={c['attendanceId']}
                    name={c['name']}
                    color={getNameButtonColor(c)}
                    onClick={props.invertSelectCandidate}
                    isOutline={!c.isSelected}
                />
            </Grid>
        ))

        return (
            <Grid size={12}>
                <Grid
                    container
                    spacing={3}
                    justifyContent="space-between"
                    alignItems="center"
                >
                    {candidates}
                    <Grid size={12}>
                        <LargeButton
                            id={'submit'}
                            name={props.checkType}
                            color={
                                areNoCandidatesSelected()
                                    ? 'secondary'
                                    : 'success'
                            }
                            onClick={props.onCheckInOutMultiple}
                            isOutline={false}
                            disabled={areNoCandidatesSelected()}
                        />
                    </Grid>
                </Grid>
            </Grid>
        )
    }

    function renderSingleCheckout() {
        const candidates = props.candidates.map((c) => (
            <Grid size={12} key={c['attendanceId']}>
                <LargeButton
                    id={c['attendanceId']}
                    name={c['name']}
                    color={getNameButtonColor(c)}
                    onClick={props.onCheckInOutSingle}
                />
            </Grid>
        ))

        return (
            <Grid size={12}>
                <Grid
                    container
                    spacing={3}
                    justifyContent="space-between"
                    alignItems="center"
                >
                    {candidates}
                </Grid>
            </Grid>
        )
    }

    function getNameButtonColor(candidate) {
        if (props.checkType === 'CheckIn') {
            return 'primary'
        }

        if (candidate['hasPeopleWithoutPickupPermission']) {
            return 'error'
        }

        if (!candidate['mayLeaveAlone']) {
            return 'warning'
        }

        return 'primary'
    }

    function areNoCandidatesSelected() {
        return props.candidates.filter((c) => c.isSelected).length <= 0
    }

    if (props.isSingleCheckInOut) {
        return renderSingleCheckout()
    }

    return renderMultiCheckout()
}
