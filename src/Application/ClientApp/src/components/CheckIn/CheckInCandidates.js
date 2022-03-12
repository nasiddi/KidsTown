import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import { LargeButton } from '../Common'

export class CheckInCandidates extends Component {
	static displayName = CheckInCandidates.name

	constructor(props) {
		super(props)
	}

	render() {
		if (this.props.isSingleCheckInOut) {
			return this.renderSingleCheckout()
		}

		return this.renderMultiCheckout()
	}

	renderMultiCheckout() {
		const candidates = this.props.candidates.map((c) => (
			<Grid item xs={12} key={c['attendanceId']}>
				<LargeButton
					id={c['attendanceId']}
					name={c['name']}
					color={this.getNameButtonColor(c)}
					onClick={this.props.invertSelectCandidate}
					isOutline={!c.isSelected}
				/>
			</Grid>
		))

		return (
			<Grid item xs={12}>
				<Grid
					container
					spacing={3}
					justifyContent="space-between"
					alignItems="center"
				>
					{candidates}
					<Grid item xs={12}>
						<LargeButton
							id={'submit'}
							name={this.props.checkType}
							color={
								this.areNoCandidatesSelected()
									? 'secondary'
									: 'success'
							}
							onClick={this.props.onCheckInOutMultiple}
							isOutline={false}
							disabled={this.areNoCandidatesSelected()}
						/>
					</Grid>
				</Grid>
			</Grid>
		)
	}

	renderSingleCheckout() {
		const candidates = this.props.candidates.map((c) => (
			<Grid item xs={12} key={c['attendanceId']}>
				<LargeButton
					id={c['attendanceId']}
					name={c['name']}
					color={this.getNameButtonColor(c)}
					onClick={this.props.onCheckInOutSingle}
				/>
			</Grid>
		))

		return (
			<Grid item xs={12}>
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

	getNameButtonColor(candidate) {
		if (this.props.checkType === 'CheckIn') {
			return 'primary'
		}

		if (candidate['hasPeopleWithoutPickupPermission']) {
			return 'danger'
		}

		if (!candidate['mayLeaveAlone']) {
			return 'warning'
		}

		return 'primary'
	}

	areNoCandidatesSelected() {
		return this.props.candidates.filter((c) => c.isSelected).length <= 0
	}
}
