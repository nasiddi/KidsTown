import React, { Component } from 'react'
import { Grid, MuiThemeProvider } from '@material-ui/core'
import { LargeButton, primaryTheme } from '../Common'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import TextField from '@material-ui/core/TextField'

export class CheckInPhoneNumbers extends Component {
	static displayName = CheckInPhoneNumbers.name
	constructor(props) {
		super(props)
	}

	render() {
		const adults = this.props.adults.map((a) => (
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
							color={
								a['isPrimaryContact'] ? 'success' : 'secondary'
							}
							isOutline={!a['isPrimaryContact']}
							onClick={this.props.onPrimaryContactChange}
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
					{this.renderPhoneNumber(
						a['personId'],
						a['phoneNumber'],
						this.getPhoneNumberIsEdit(a['personId'])
					)}
					{this.renderPhoneNumberEditButton(
						a['personId'],
						a['phoneNumber'],
						this.getPhoneNumberIsEdit(a['personId'])
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

	renderPhoneNumberEditButton(id, phoneNumber, isEdit) {
		if (isEdit) {
			return (
				<Grid item md={2} xs={4}>
					<LargeButton
						id={id}
						name="Save"
						color="success"
						onClick={this.props.onSave}
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
					onClick={this.props.onEdit}
				/>
			</Grid>
		)
	}

	renderPhoneNumber(id, phoneNumber, isEdit) {
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
							onChange={this.props.onChange}
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

	getPhoneNumberIsEdit(personId) {
		const flag = _.find(this.props.phoneNumberEditFlags, { id: personId })

		return flag['isEdit']
	}
}
