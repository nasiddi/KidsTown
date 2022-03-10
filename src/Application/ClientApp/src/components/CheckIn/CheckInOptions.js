import React from 'react'
import { Grid } from '@material-ui/core'
import { MultiSelect, PrimaryCheckBox, ToggleButtons } from '../Common'

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
		<Grid item xs={12}>
			<Grid
				container
				spacing={1}
				justify="space-between"
				alignItems="center"
			>
				<Grid item md={'auto'} xs={12}>
					<ToggleButtons buttons={toggleButtons} />
				</Grid>
				<Grid item>
					<PrimaryCheckBox
						name="fastCheckInOut"
						checked={props.fastCheckInOut}
						onChange={props.onCheckBoxChange}
						label={`Fast ${props.checkType}`}
					/>
				</Grid>
				<Grid item>
					<PrimaryCheckBox
						name="singleCheckInOut"
						checked={props.singleCheckInOut}
						onChange={props.onCheckBoxChange}
						label={`Single ${props.checkType}`}
					/>
				</Grid>
				<Grid item>
					<PrimaryCheckBox
						name="showPhoneNumbers"
						checked={props.showPhoneNumbers}
						onChange={props.onCheckBoxChange}
						label={'Show Numbers'}
						disabled={props.checkType !== 'CheckIn'}
					/>
				</Grid>
				<Grid item md={'auto'} xs={12} style={{ minWidth: '190px' }}>
					<MultiSelect
						name={'checkInOutLocationGroups'}
						onChange={props.onLocationGroupChange}
						isMulti={true}
						options={props.locationGroups}
						defaultOptions={props.checkInOutLocationGroups}
						minHeight={44}
					/>
				</Grid>
			</Grid>
		</Grid>
	)
}
