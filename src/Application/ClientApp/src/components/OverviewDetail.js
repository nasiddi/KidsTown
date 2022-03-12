import React, { Component } from 'react'
import {
	Accordion,
	AccordionDetails,
	AccordionSummary,
	Grid,
	Typography,
} from '@material-ui/core'
import {
	getLastSunday,
	getSelectedEventFromStorage,
	getSelectedFromSession,
	getSelectedOptionsFromStorage,
	getStringFromSession,
	HtmlTooltip,
} from './Common'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { Badge, Table } from 'reactstrap'
import ExpandMoreIcon from '@material-ui/icons/ExpandMore'
import { withAuth } from '../auth/MsalAuthProvider'

class Detail extends Component {
	static displayName = Detail.name
	repeat

	constructor(props) {
		super(props)

		this.state = {
			attendees: [],
			loading: true,
		}
	}

	async componentDidMount() {
		await this.fetchData()
		this.setState({ loading: false })
	}

	componentWillUnmount() {
		clearTimeout(this.repeat)
	}

	renderDetails() {
		return (
			<Grid container spacing={3}>
				{this.state.attendees.map((attendees) => (
					<Grid item xs={12} key={attendees['location']}>
						<Accordion
							className="overview-accordion"
							defaultExpanded
						>
							<AccordionSummary
								expandIcon={<ExpandMoreIcon />}
								aria-controls="panel1a-content"
								id="panel1a-header"
							>
								<h3>{attendees['location']}</h3>
							</AccordionSummary>
							<AccordionDetails>
								<Grid container spacing={1}>
									<Grid item xs={12} md={8}>
										{this.renderKidsTable(
											attendees['kids']
										)}
									</Grid>
									<Grid item xs={12} md={4}>
										{this.renderVolunteerTable(
											attendees['volunteers']
										)}
									</Grid>
								</Grid>
							</AccordionDetails>
						</Accordion>
					</Grid>
				))}
			</Grid>
		)
	}

	renderKidsTable(kids) {
		const selectedState = getSelectedFromSession('selectedOverviewStates', [
			'PreCheckedIn',
			'CheckedIn',
			'CheckedOut',
		])

		const filteredKids = _.filter(kids, (p) =>
			_.includes(selectedState, p['checkState'])
		)

		return (
			<div>
				<h4>Kinder</h4>
				<Table>
					<thead>
						<tr>
							<th>First Name</th>
							<th>Last Name</th>
							<th>
								<div style={{ textAlign: 'center' }}>
									<FontAwesomeIcon
										icon={['fas', 'mobile-alt']}
									/>
								</div>
							</th>
							<th>Status</th>
							<th>SecurityCode</th>
						</tr>
					</thead>
					<tbody>
						{filteredKids.map((row) => (
							<tr key={row['attendanceId']}>
								<td>{row['firstName']}</td>
								<td>{row['lastName']}</td>
								<td>
									<div
										id={this.getToolTipTarget(row)}
										style={{ textAlign: 'center' }}
									>
										{this.getTooltip(row)}
									</div>
								</td>
								<td>{this.getStateBadge(row['checkState'])}</td>
								<td>{row['securityCode']}</td>
							</tr>
						))}
					</tbody>
				</Table>
			</div>
		)
	}

	getTooltip(row) {
		if (row['adults'].length === 0) {
			return <div />
		}

		return (
			<HtmlTooltip
				arrow
				enterTouchDelay={0}
				leaveTouchDelay={10000}
				title={
					<React.Fragment>{this.getAdultInfos(row)}</React.Fragment>
				}
			>
				<span>
					<FontAwesomeIcon icon={['fas', 'mobile-alt']} />
				</span>
			</HtmlTooltip>
		)
	}

	getToolTipTarget(row) {
		return `${row['firstName'].replace(/\s+/g, '')}${row[
			'lastName'
		].replace(/\s+/g, '')}${row['attendanceId']}`
	}

	renderVolunteerTable(volunteers) {
		return (
			<div>
				<h4>Betreuer</h4>
				<Table responsive>
					<thead>
						<tr>
							<th>First Name</th>
							<th>Last Name</th>
						</tr>
					</thead>
					<tbody>
						{volunteers.map((row) => (
							<tr key={row['attendanceId']}>
								<td>{row['firstName']}</td>
								<td>{row['lastName']}</td>
							</tr>
						))}
					</tbody>
				</Table>
			</div>
		)
	}

	render() {
		if (this.state.loading) {
			return <div />
		}

		return (
			<div>
				<Grid
					container
					spacing={3}
					justifyContent="space-between"
					alignItems="flex-start"
				>
					<Grid item xs={12}>
						{this.renderDetails()}
					</Grid>
				</Grid>
			</div>
		)
	}

	async fetchData() {
		await fetch(
			`overview/event/${await getSelectedEventFromStorage()}/attendees?date=${getStringFromSession(
				'overviewDate',
				getLastSunday().toISOString()
			)}`,
			{
				body: JSON.stringify(
					getSelectedOptionsFromStorage(
						'overviewLocationGroups',
						[]
					).map((l) => l.value)
				),
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
			}
		)
			.then((r) => r.json())
			.then((j) => {
				this.setState({ attendees: j })
			})

		this.repeat = setTimeout(this.fetchData.bind(this), 500)
	}

	getStateBadge(state) {
		let color = ''

		if (state === 'PreCheckedIn') {
			color = 'info'
		}

		if (state === 'CheckedIn') {
			color = 'success'
		}

		if (state === 'CheckedOut') {
			color = 'primary'
		}

		return <Badge color={color}>{state}</Badge>
	}

	getAdultInfos(row) {
		return (
			<div>
				{row['adults'].map((a) => {
					const primary = a['isPrimaryContact'] ? (
						<FontAwesomeIcon icon="star" />
					) : (
						<div />
					)

					return (
						<Typography
							key={`${a['firstName']} ${a['lastName']} ${a['phoneNumber']}`}
						>
							{primary}{' '}
							{`${a['firstName']} ${a['lastName']} ${a['phoneNumber']}`}
							<br />
						</Typography>
					)
				})}
			</div>
		)
	}
}

export const OverviewDetail = withAuth(Detail)
