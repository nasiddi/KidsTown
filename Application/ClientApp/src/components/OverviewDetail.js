import React, { Component } from 'react'
import {
	Accordion,
	AccordionDetails,
	AccordionSummary,
	Grid,
} from '@material-ui/core'
import {
	getSelectedEventFromStorage,
	getStringFromSession,
	getSelectedOptionsFromStorage,
	getLastSunday,
} from './Common'
import { Table } from 'reactstrap'
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
						<Accordion className="overview-accordion">
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
		return (
			<div>
				<h4>Kinder</h4>
				<Table responsive>
					<thead>
						<tr>
							<th>First Name</th>
							<th>Last Name</th>
							<th>Status</th>
							<th>SecurityCode</th>
						</tr>
					</thead>
					<tbody>
						{kids.map((row) => (
							<tr key={row['checkInId']}>
								<td>{row['firstName']}</td>
								<td>{row['lastName']}</td>
								<td>{row['checkState']}</td>
								<td>{row['securityCode']}</td>
							</tr>
						))}
					</tbody>
				</Table>
			</div>
		)
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
							<tr key={row['checkInId']}>
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
					justify="space-between"
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
					getSelectedOptionsFromStorage('overviewLocations', []).map(
						(l) => l.value
					)
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
}

export const OverviewDetail = withAuth(Detail)
