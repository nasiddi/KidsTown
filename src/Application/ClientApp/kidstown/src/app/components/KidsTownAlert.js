'use client'
import React from 'react'
import {Alert, IconButton} from '@mui/material'
import Grid from '@mui/material/Grid2';
import ReplayIcon from '@mui/icons-material/Replay'

export function KidsTownAlert(props) {
    return (
        <Grid size={12}>
            <Alert
                severity={props.alert.level.toLowerCase()}
                action={
                    props.showUndoLink ? (
                        <IconButton color="inherit" onClick={props.onUndo}>
                            <ReplayIcon onClick={props.callback}/>
                        </IconButton>
                    ) : (
                        <></>
                    )
                }
            >
                {props.alert.text}
            </Alert>
        </Grid>
    )
}
