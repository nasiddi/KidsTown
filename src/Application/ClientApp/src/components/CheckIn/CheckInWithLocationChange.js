import React from 'react'
import { FormControl, Grid, InputLabel, MenuItem, Select } from '@mui/material'
import { LargeButton } from '../Common'

export function CheckInWithLocationChange(props) {
	const candidates = props.candidates

	if (candidates.length === 0) {
		return (
			<Grid item xs={12}>
				<LargeButton
					id={'unfilteredSearch'}
					name={'Suche ohne Location Filter'}
					color={'warning'}
					onClick={props.onSearch}
				/>
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
					spacing={2}
					justifyContent="space-between"
					alignItems="center"
				>
					{candidateButtons}
					<Grid item xs={12}>
						<FormControl fullWidth>
							<InputLabel>Select Location</InputLabel>
							<Select
								name={'changeLocationGroup'}
								onChange={props.onLocationGroupChange}
								label={'Select Location'}
								value={props.selectedLocationGroupId ?? ''}
							>
								{props.locationGroups.map((l) => (
									<MenuItem value={l.value} key={l.value}>
										{l.label}
									</MenuItem>
								))}
							</Select>
						</FormControl>
					</Grid>
					{props.locations.length > 1 ? (
						<Grid item xs={12}>
							<FormControl fullWidth>
								<InputLabel>Select SubLocation</InputLabel>
								<Select
									name={'changeLocation'}
									onChange={props.onLocationChange}
									label={'Select SubLocation'}
									value={props.selectedLocation.value ?? ''}
								>
									{props.locations.map((l) => (
										<MenuItem value={l.value} key={l.value}>
											{l.label}
										</MenuItem>
									))}
								</Select>
							</FormControl>
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
				justifyContent="space-between"
				alignItems="center"
			>
				{candidateButtons}
			</Grid>
		</Grid>
	)
}
