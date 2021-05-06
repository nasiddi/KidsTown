import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import Checkbox from '@material-ui/core/Checkbox'
import FormControlLabel from '@material-ui/core/FormControlLabel'
import { getSelectedEventFromStorage } from './Common'
import { withAuth } from '../auth/MsalAuthProvider'
import { Badge, Button, Card, CardBody, CardTitle } from 'reactstrap'

function BoolBadge(props) {
	return <Badge color={props['color']}>{props['label']}</Badge>
}

function Task(props) {
	const task = props['task']

	function GetBadge(isActive, trueColor, trueLabel, falseColor, falseLabel) {
		if (isActive === true) {
			return <BoolBadge color={trueColor} label={trueLabel} />
		}

		return <BoolBadge color={falseColor} label={falseLabel} />
	}

	return (
		<Card>
			<CardBody>
				<CardTitle tag="h5">{task['backgroundTaskType']}</CardTitle>

				<CardTitle tag="h5">
					{GetBadge(
						task['isActive'],
						'primary',
						'active',
						'secondary',
						'inactive'
					)}{' '}
					{GetBadge(
						task['isEnabled'],
						'primary',
						'enabled',
						'secondary',
						'disabled'
					)}{' '}
					{GetBadge(
						task['taskRunsSuccessfully'],
						'success',
						'success',
						'danger',
						'failed'
					)}
				</CardTitle>

				<Grid
					container
					spacing={1}
					justify="space-between"
					alignItems="center"
				>
					<Grid item xs={6} />
				</Grid>
				<Button>Edit</Button>
			</CardBody>
		</Card>
	)
}

class Setting extends Component {
	static displayName = Setting.name

	repeat

	constructor(props) {
		super(props)

		this.state = {
			events: [],
			tasks: [],
			selectedEvent: 0,
			loading: true,
		}
	}

	async componentDidMount() {
		await this.fetchAvailableEvents()
		await this.fetchTasks()
		this.setState({ loading: false })
	}

	componentWillUnmount() {
		clearTimeout(this.repeat)
	}

	renderEvents() {
		const events = this.state.events.map((e) => (
			<Grid item xs={12} key={e['eventId']}>
				<div className="event">
					<FormControlLabel
						control={
							<Checkbox
								name={e['name']}
								color="primary"
								onChange={this.handleChange}
								checked={
									this.state.selectedEvent === e['eventId']
								}
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
					justify="space-between"
					alignItems="center"
				>
					{events}
				</Grid>
			</div>
		)
	}

	renderTasks() {
		if (this.state.loading) {
			return <div />
		}

		const tasks = this.state.tasks.map((t) => (
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
					justify="space-between"
					alignItems="center"
				>
					{tasks}
				</Grid>
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
						<h1>Settings</h1>
					</Grid>
					<Grid item xs={4}>
						{this.renderEvents()}
					</Grid>
					<Grid item xs={8}>
						{this.renderTasks()}
					</Grid>
				</Grid>
			</div>
		)
	}

	updateOptions = async (options, key) => {
		localStorage.setItem(key, JSON.stringify(options))
		this.setState({ [key]: options })
	}

	async fetchAvailableEvents() {
		const response = await fetch('configuration/events')
		const events = await response.json()
		const selected = await getSelectedEventFromStorage()
		this.setState({ events: events, selectedEvent: selected })
	}

	async fetchTasks() {
		const response = await fetch('configuration/tasks')
		const tasks = await response.json()
		this.setState({ tasks: tasks })
	}

	handleChange = (event) => {
		if (event.target.checked) {
			const selected = this.state.events.find(
				(e) => e['name'] === event.target.name
			)
			localStorage.setItem('selectedEvent', selected['eventId'])
			this.setState({ selectedEvent: selected['eventId'] })
		}
	}
}

export const Settings = withAuth(Setting)
