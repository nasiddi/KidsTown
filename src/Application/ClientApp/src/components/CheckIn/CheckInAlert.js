import React from 'react'
import { Grid, Alert, IconButton } from '@mui/material'
import ReplayIcon from '@mui/icons-material/Replay'

export function CheckInAlert(props) {
	return (
		<Grid item xs={12}>
			<Alert
				severity={props.alert.level.toLowerCase()}
				action={
					props.showUndoLink ? (
						<IconButton color="inherit" onClick={props.onUndo}>
							<ReplayIcon onClick={props.callback} />
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
