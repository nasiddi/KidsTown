import React, { useEffect, useState } from 'react'
import {
	Grid,
	Checkbox,
	FormControlLabel,
	Chip,
	Card,
	CardContent,
	CardHeader,
} from '@mui/material'
import { getGuid, getSelectedEventFromStorage } from './Common'
import { withAuth } from '../auth/MsalAuthProvider'
import { NarrowLayout } from './Layout'

function BoolBadge(props) {
	return <Chip color={props['color']} label={props['label']} />
}

function Task(props) {
	const task = props['task']

	function GetBadge(isActive, trueColor, trueLabel, falseColor, falseLabel) {
		if (isActive === true) {
			return <BoolBadge color={trueColor} label={trueLabel} />
		}

		return <BoolBadge color={falseColor} label={falseLabel} />
	}

	const failedCount = []

	if (!task['taskRunsSuccessfully']) {
		failedCount.push(
			<Grid item xs={12}>
				<hr />
			</Grid>
		)
		failedCount.push(
			<Grid item md={6} xs={12}>
				<b>Failed Executions</b>
			</Grid>
		)
		failedCount.push(
			<Grid item md={6} xs={12}>
				{task['currentFailCount']}
			</Grid>
		)
	}

	return (
		<Card>
			<CardHeader title={task['backgroundTaskType']} />
			<CardContent>
				<Grid
					container
					spacing={1}
					justifyContent="space-between"
					alignItems="center"
				>
					<Grid item md={6} xs={12}>
						{GetBadge(
							task['isActive'],
							'primary',
							'active',
							'secondary',
							'inactive'
						)}{' '}
						{GetBadge(
							task['taskRunsSuccessfully'],
							'success',
							'success',
							'error',
							'failed'
						)}
					</Grid>
					<Grid item xs={12}>
						<hr />
					</Grid>
					<Grid item md={6} xs={12}>
						<b>Last Execution</b>
					</Grid>
					<Grid item md={6} xs={12}>
						{new Date(task['lastExecution']).toLocaleString(
							'de-Ch'
						)}
					</Grid>
					<Grid item xs={12}>
						<hr />
					</Grid>
					<Grid item md={6} xs={12}>
						<b>Successful Executions</b>
					</Grid>
					<Grid item md={6} xs={12}>
						{task['successCount']}
					</Grid>
					{failedCount}
					<Grid item xs={12}>
						<hr />
					</Grid>
					<Grid item md={6} xs={12}>
						<b>Interval</b>
					</Grid>
					<Grid item md={6} xs={12}>
						{task['interval']} milliseconds
					</Grid>
					<Grid item xs={12}>
						<hr />
					</Grid>
					<Grid item md={6} xs={12}>
						<b>Log Frequency</b>
					</Grid>
					<Grid item md={6} xs={12}>
						Every {task['logFrequency']} executions
					</Grid>
				</Grid>
			</CardContent>
		</Card>
	)
}

function Setting() {
	const [state, setState] = useState({
		events: [],
		tasks: [],
		selectedEvent: 0,
		loading: true,
	})

	useEffect(() => {
		async function load() {
			setState({
				...state,
				selectedEvent: await getSelectedEventFromStorage(),
				events: await fetchAvailableEvents(),
				tasks: await fetchTasks(),
				loading: false,
			})
		}

		load().then()
	}, [])

	async function fetchAvailableEvents() {
		const response = await fetch('configuration/events')

		return await response.json()
	}

	async function fetchTasks() {
		const response = await fetch('configuration/tasks')

		return await response.json()
	}

	const handleChange = (event) => {
		if (event.target.checked) {
			const selected = state.events.find(
				(e) => e['name'] === event.target.name
			)
			localStorage.setItem('selectedEvent', selected['eventId'])
			localStorage.setItem('statisticLocations', JSON.stringify([]))
			setState({ ...state, selectedEvent: selected['eventId'] })
		}
	}

	function renderEvents() {
		const events = state.events.map((e) => (
			<Grid item xs={12} key={e['eventId']}>
				<div className="event">
					<FormControlLabel
						control={
							<Checkbox
								name={e['name']}
								color="primary"
								onChange={handleChange}
								checked={state.selectedEvent === e['eventId']}
							/>
						}
						label={''}
						labelPlacement="end"
					/>
					<a
						href={`https://check-ins.planningcenteronline.com/events/${e['eventId']}`}
						target="_blank"
						rel="noopener noreferrer"
					>
						{e['name']}
					</a>
				</div>
			</Grid>
		))

		return (
			<div>
				<h2>Events</h2>
				<Grid
					container
					spacing={0}
					justifyContent="space-between"
					alignItems="center"
				>
					{events}
				</Grid>
			</div>
		)
	}

	function renderTasks() {
		if (state.loading) {
			return <div />
		}

		const tasks = state.tasks.map((t) => (
			<Grid item xs={12} key={t['backgroundTaskType']}>
				<Task task={t} />
			</Grid>
		))

		return (
			<div>
				<h2>Tasks</h2>
				<Grid
					container
					spacing={1}
					justifyContent="space-between"
					alignItems="center"
				>
					{tasks}
				</Grid>
			</div>
		)
	}

	if (state.loading) {
		return <div />
	}

	return (
		<NarrowLayout>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
				alignItems="flex-start"
			>
				<Grid item xs={12}>
					<h1>Settings</h1>
				</Grid>
				<Grid item xs={12}>
					<h3>Device</h3>
					<p>{getGuid()}</p>
				</Grid>
				<Grid item xs={4}>
					{renderEvents()}
				</Grid>
				<Grid item xs={8}>
					{renderTasks()}
				</Grid>
			</Grid>
		</NarrowLayout>
	)
}

export const Settings = withAuth(Setting)
