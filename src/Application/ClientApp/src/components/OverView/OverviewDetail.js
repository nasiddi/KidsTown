import React, { useState, useEffect } from 'react'
import {
	Accordion,
	AccordionDetails,
	AccordionSummary,
	Grid,
	Typography,
	Chip,
	Table,
	TableCell,
	TableHead,
	TableRow,
	TableBody,
} from '@mui/material'
import {
	getLastSunday,
	getSelectedEventFromStorage,
	getSelectedFromSession,
	getSelectedOptionsFromStorage,
	getStringFromSession,
	HtmlTooltip,
	TableHeadCell,
} from '../Common'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import PhoneIphoneIcon from '@mui/icons-material/PhoneIphone'
import StarIcon from '@mui/icons-material/Star'
import { styled } from '@mui/material/styles'

const StyledTableCell = styled(TableCell)({
	height: 37,
})

export default function OverviewDetail() {
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
		const filteredKids = kids.filter((k) =>
			selectedState.includes(k.checkState)
		)

		return (
			<div>
				<h4>Kinder</h4>
				<Table size="small">
					<TableHead>
						<TableRow>
							<TableHeadCell>
								<strong>First Name</strong>
							</TableHeadCell>
							<TableHeadCell>
								<strong>Last Name</strong>
							</TableHeadCell>
							<TableHeadCell>
								<div style={{ textAlign: 'center' }}>
									<PhoneIphoneIcon fontSize={'small'} />
								</div>
							</TableHeadCell>
							<TableHeadCell style={{ width: 100 }}>
								<strong>Status</strong>
							</TableHeadCell>
							<TableHeadCell>
								<strong>SecurityCode</strong>
							</TableHeadCell>
						</TableRow>
					</TableHead>
					<TableBody>
						{filteredKids.map((row) => (
							<TableRow key={row['attendanceId']}>
								<TableCell>{row['firstName']}</TableCell>
								<TableCell>{row['lastName']}</TableCell>
								<TableCell>
									<div
										id={getToolTipTarget(row)}
										style={{ textAlign: 'center' }}
									>
										{getTooltip(row)}
									</div>
								</TableCell>
								<TableCell style={{ width: 100 }}>
									{getStateBadge(row['checkState'])}
								</TableCell>
								<TableCell>{row['securityCode']}</TableCell>
							</TableRow>
						))}
					</TableBody>
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
				<PhoneIphoneIcon
					style={{
						border: 'none',
						backgroundColor: 'white',
						color: 'black',
					}}
					fontSize={'small'}
				/>
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
				<Table size="small">
					<TableHead>
						<TableRow>
							<TableHeadCell>
								<strong>First Name</strong>
							</TableHeadCell>
							<TableHeadCell>
								<strong>Last Name</strong>
							</TableHeadCell>
						</TableRow>
					</TableHead>
					<TableBody>
						{volunteers.map((row) => (
							<TableRow key={row['attendanceId']}>
								<StyledTableCell>
									{row['firstName']}
								</StyledTableCell>
								<StyledTableCell>
									{row['lastName']}
								</StyledTableCell>
							</TableRow>
						))}
					</TableBody>
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

		// noinspection JSValidateTypes
		return (
			<Chip
				color={color}
				label={state}
				size="small"
				style={{ width: '-webkit-fill-available' }}
			/>
		)
	}

	function getAdultInfos(row) {
		return (
			<span>
				{row['adults'].map((a) => {
					const primary = a['isPrimaryContact'] ? (
						<StarIcon />
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
			</span>
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
