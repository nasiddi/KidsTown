import React from 'react'
import { Grid } from '@material-ui/core'
import { UndoButton } from '../Common'
import { Alert } from 'reactstrap'

export function CheckInAlert(props) {
	const undoLink = props.showUndoLink ? (
		<UndoButton callback={props.onUndo} />
	) : (
		<div />
	)

	return (
		<Grid item xs={12}>
			<Alert color={props.alert.level.toLowerCase()}>
				<Grid
					container
					direction="row"
					justifyContent="space-between"
					alignItems="center"
					spacing={1}
				>
					<Grid item xs={11}>
						{props.alert.text}
					</Grid>
					<Grid item xs={1}>
						{undoLink}
					</Grid>
				</Grid>
			</Alert>
		</Grid>
	)
}
