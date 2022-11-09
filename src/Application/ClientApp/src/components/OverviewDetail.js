import React, { useState, useEffect } from 'react'
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

function Detail() {
	let repeat = undefined

	const [state, setState] = useState({
		attendees: [],
		loading: true,
	})

	useEffect(() => {
		async function load() {
			await fetchData()
		}

		load().then()

		return () => {
			clearTimeout(repeat)
		}
	}, [])

	function renderDetails() {
		return (
			<Grid container spacing={3}>
				{state.attendees.map((attendees) => (
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
										{renderKidsTable(attendees['kids'])}
									</Grid>
									<Grid item xs={12} md={4}>
										{renderVolunteerTable(
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

	function renderKidsTable(kids) {
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
										id={getToolTipTarget(row)}
										style={{ textAlign: 'center' }}
									>
										{getTooltip(row)}
									</div>
								</td>
								<td>{getStateBadge(row['checkState'])}</td>
								<td>{row['securityCode']}</td>
							</tr>
						))}
					</tbody>
				</Table>
			</div>
		)
	}

	function getTooltip(row) {
		if (row['adults'].length === 0) {
			return <div />
		}

		return (
			<HtmlTooltip
				arrow
				enterTouchDelay={0}
				leaveTouchDelay={10000}
				title={<React.Fragment>{getAdultInfos(row)}</React.Fragment>}
			>
				<span>
					<FontAwesomeIcon icon={['fas', 'mobile-alt']} />
				</span>
			</HtmlTooltip>
		)
	}

	function getToolTipTarget(row) {
		return `${row['firstName'].replace(/\s+/g, '')}${row[
			'lastName'
		].replace(/\s+/g, '')}${row['attendanceId']}`
	}

	function renderVolunteerTable(volunteers) {
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

	async function fetchData() {
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
				setState({ ...state, attendees: j, loading: false })
			})

		repeat = setTimeout(fetchData.bind(this), 500)
	}

	function getStateBadge(state) {
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

	function getAdultInfos(row) {
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

	if (state.loading) {
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
					{renderDetails()}
				</Grid>
			</Grid>
		</div>
	)
}

export const OverviewDetail = withAuth(Detail)
