'use client'
import React, {useEffect, useState} from 'react'
import Grid from '@mui/material/Grid2';
import {getSelectedFromSession, StyledButton} from '../components/Common'

function FilterButton(props) {
    return (
        <StyledButton
            color={props['color']}
            onClick={props['onClick']}
            variant={!props['isSelected'] ? 'outlined' : 'contained'}
            fullWidth={true}
            size={'large'}
        >
            {props['label']}
        </StyledButton>
    )
}

export default function OverviewFilter() {
    const [state, setState] = useState({
        selectedStates: [
            'PreCheckedIn',
            'CheckedIn',
            'CheckedOut',
        ],
    })

    useEffect(() => {

        setState({
            ...state,
            selectedStates: getSelectedFromSession('selectedOverviewStates', state.selectedStates),
        })
    }, [])


    function stateIsSelected(target) {
        return state.selectedStates.includes(target)
    }

    function renderFilter() {
        return (
            <div>
                <Grid
                    container
                    spacing={3}
                    justifyContent="space-between"
                    alignItems="center"
                >
                    <Grid size={12}>
                        <h3/>
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
                            color={'primary'}
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

    function toggleState(toggledState) {
        let currentStates = state.selectedStates
        if (currentStates.includes(toggledState)) {
            currentStates = currentStates.filter((p) => p !== toggledState)
        } else {
            currentStates.push(toggledState)
        }

        setState({...state, selectedStates: currentStates})
        sessionStorage.setItem(
            'selectedOverviewStates',
            JSON.stringify(currentStates)
        )
    }

    return (
        <div>
            <Grid
                container
                spacing={3}
                justifyContent="space-between"
                alignItems="flex-start"
            >
                <Grid size={12}>
                    {renderFilter()}
                </Grid>
            </Grid>
        </div>
    )
}
