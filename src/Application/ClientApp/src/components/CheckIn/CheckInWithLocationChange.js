import React from 'react'
import { Grid } from '@material-ui/core'
import { LargeButton, MultiSelect } from '../Common'
import { Fade } from 'reactstrap'

export function CheckInWithLocationChange(props) {
	const candidates = props.candidates

	if (candidates.length === 0) {
		return (
			<Grid item xs={12}>
				<Fade in={props.fadeIn}>
					<LargeButton
						id={'unfilteredSearch'}
						name={'Suche ohne Location Filter'}
						color={'warning'}
						onClick={props.onSearch}
						isOutline={false}
					/>
				</Fade>
			</Grid>
		)
	}

	const candidateButtons = candidates.map((c) => (
		<Grid item xs={12} key={c['attendanceId']}>
			<LargeButton
				id={c['attendanceId']}
				name={c['name']}
				color={'primary'}
				onClick={props.onSelectCandidate}
			/>
		</Grid>
	))

	if (candidates.length === 1) {
		return (
			<Grid item xs={12}>
				<Grid
					container
					spacing={1}
					justify="space-between"
					alignItems="center"
				>
					{candidateButtons}
					<Grid item xs={12}>
						<MultiSelect
							name={'changeLocationGroup'}
							onChange={props.onLocationGroupChange}
							isMulti={false}
							options={props.locationGroups}
							minHeight={44}
						/>
					</Grid>
					{props.locations.length > 1 ? (
						<Grid item xs={12}>
							<MultiSelect
								name={'changeLocation'}
								onChange={props.onLocationChange}
								isMulti={false}
								options={props.locations}
								minHeight={44}
							/>
						</Grid>
					) : (
						<div />
					)}
					{props.selectedLocation.value !== undefined ? (
						<Grid item xs={12}>
							<LargeButton
								id={'checkinWithLocationChange'}
								name={`CheckIn ${candidates[0]['name']} \u{2192} ${props.selectedLocation.label}`}
								color={'success'}
								onClick={props.onCheckIn}
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
		<Grid item xs={12}>
			<Grid
				container
				spacing={3}
				justify="space-between"
				alignItems="center"
			>
				{candidateButtons}
			</Grid>
		</Grid>
	)
}
