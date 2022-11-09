import React from 'react'
import { Grid, MuiThemeProvider } from '@material-ui/core'
import { LargeButton, primaryTheme } from '../Common'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import TextField from '@material-ui/core/TextField'

export function CheckInPhoneNumbers(props) {
	function renderPhoneNumberEditButton(id, phoneNumber, isEdit) {
		if (isEdit) {
			return (
				<Grid item md={2} xs={4}>
					<LargeButton
						id={id}
						name="Save"
						color="success"
						onClick={props.onSave}
					/>
				</Grid>
			)
		}

		return (
			<Grid item md={2} xs={4}>
				<LargeButton
					id={id}
					name="Edit"
					color="primary"
					onClick={props.onEdit}
				/>
			</Grid>
		)
	}

	function renderPhoneNumber(id, phoneNumber, isEdit) {
		if (isEdit) {
			return (
				<Grid item md={3} xs={8}>
					<MuiThemeProvider theme={primaryTheme}>
						<TextField
							id={id}
							label="PhoneNumber"
							variant="outlined"
							value={phoneNumber}
							fullWidth={true}
							onChange={props.onChange}
						/>
					</MuiThemeProvider>
				</Grid>
			)
		}

		return (
			<Grid item md={3} xs={8}>
				<h4
					style={{
						justifyContent: 'center',
						height: '100%',
						margin: 0,
					}}
				>
					{phoneNumber}
				</h4>
			</Grid>
		)
	}

	function getPhoneNumberIsEdit(personId) {
		const flag = _.find(props.phoneNumberEditFlags, { id: personId })

		return flag['isEdit']
	}

	const adults = props.adults.map((a) => (
		<Grid item xs={12} key={a['personId']}>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
				alignItems="center"
			>
				<Grid item md={1} xs={3}>
					<LargeButton
						id={a['personId']}
						name={
							<span id={a['personId']}>
								<FontAwesomeIcon icon="star" />
							</span>
						}
						color={a['isPrimaryContact'] ? 'success' : 'secondary'}
						isOutline={!a['isPrimaryContact']}
						onClick={props.onPrimaryContactChange}
					/>
				</Grid>
				<Grid item md={6} xs={9}>
					<h4
						style={{
							justifyContent: 'center',
							height: '100%',
							margin: 0,
						}}
					>
						{`${a['firstName']} ${a['lastName']}`}
					</h4>
				</Grid>
				{renderPhoneNumber(
					a['personId'],
					a['phoneNumber'],
					getPhoneNumberIsEdit(a['personId'])
				)}
				{renderPhoneNumberEditButton(
					a['personId'],
					a['phoneNumber'],
					getPhoneNumberIsEdit(a['personId'])
				)}
			</Grid>
		</Grid>
	))

	return (
		<Grid
			container
			spacing={3}
			justifyContent="space-between"
			alignItems="center"
		>
			{adults}
		</Grid>
	)
}
