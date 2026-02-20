import React, { useEffect, useState } from 'react'
import {
  Button,
  Card,
  CardContent,
  CardHeader,
  Checkbox,
  Chip,
  FormControlLabel,
} from '@mui/material'
import Grid from '@mui/material/Grid'
import { getGuid, getSelectedEventFromStorage } from '../components/Common'
import { NarrowLayout } from '../components/Layout'
import { get } from '../helpers/BackendClient'
import { useNavigate } from 'react-router-dom'
import withAuth from '../components/withAuth'

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
      <Grid size={12} key="fc-hr-1">
        <hr />
      </Grid>,
    )
    failedCount.push(
      <Grid size={{ md: 6, xs: 12 }} key="fc-label">
        <b>Failed Executions</b>
      </Grid>,
    )
    failedCount.push(
      <Grid size={{ md: 6, xs: 12 }} key="fc-value">
        {task['currentFailCount']}
      </Grid>,
    )
  }

  return (
    <Card>
      <CardHeader title={task['backgroundTaskType']} />
      <CardContent>
        <Grid container spacing={1} justifyContent="space-between" alignItems="center">
          <Grid size={{ md: 6, xs: 12 }}>
            {GetBadge(task['isActive'], 'primary', 'active', 'secondary', 'inactive')}{' '}
            {GetBadge(task['taskRunsSuccessfully'], 'success', 'success', 'error', 'failed')}
          </Grid>

          <Grid size={12}>
            <hr />
          </Grid>

          <Grid size={{ md: 6, xs: 12 }}>
            <b>Last Execution</b>
          </Grid>
          <Grid size={{ md: 6, xs: 12 }}>
            {new Date(task['lastExecution']).toLocaleString('de-CH')}
          </Grid>

          <Grid size={12}>
            <hr />
          </Grid>

          <Grid size={{ md: 6, xs: 12 }}>
            <b>Successful Executions</b>
          </Grid>
          <Grid size={{ md: 6, xs: 12 }}>{task['successCount']}</Grid>

          {failedCount}

          <Grid size={12}>
            <hr />
          </Grid>

          <Grid size={{ md: 6, xs: 12 }}>
            <b>Interval</b>
          </Grid>
          <Grid size={{ md: 6, xs: 12 }}>{task['interval']} milliseconds</Grid>

          <Grid size={12}>
            <hr />
          </Grid>

          <Grid size={{ md: 6, xs: 12 }}>
            <b>Log Frequency</b>
          </Grid>
          <Grid size={{ md: 6, xs: 12 }}>Every {task['logFrequency']} executions</Grid>
        </Grid>
      </CardContent>
    </Card>
  )
}

function Setting() {
  const navigate = useNavigate()

  const [state, setState] = useState({
    events: [],
    tasks: [],
    selectedEvent: 0,
    loading: true,
  })

  useEffect(() => {
    async function load() {
      const selectedEvent = await getSelectedEventFromStorage()
      const events = await fetchAvailableEvents()
      const tasks = await fetchTasks()

      setState((prev) => ({
        ...prev,
        selectedEvent,
        events,
        tasks,
        loading: false,
      }))
    }

    load().catch(() => {
      setState((prev) => ({ ...prev, loading: false }))
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function fetchAvailableEvents() {
    const response = await get('configuration/events')
    return await response.json()
  }

  async function fetchTasks() {
    const response = await get('configuration/tasks')
    return await response.json()
  }

  const handleChange = (event) => {
    if (event.target.checked) {
      const selected = state.events.find((e) => e['name'] === event.target.name)
      if (!selected) return

      localStorage.setItem('selectedEvent', selected['eventId'])
      localStorage.setItem('statisticLocations', JSON.stringify([]))
      setState((prev) => ({ ...prev, selectedEvent: selected['eventId'] }))
    }
  }

  const onRouteChange = () => {
    navigate('/settings/documentation')
  }

  function renderEvents() {
    const events = state.events.map((e) => (
      <Grid size={12} key={e['eventId']}>
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
        <Grid container spacing={0} justifyContent="space-between" alignItems="center">
          {events}
        </Grid>
      </div>
    )
  }

  function renderTasks() {
    if (state.loading) return <div />

    const tasks = state.tasks.map((t) => (
      <Grid size={12} key={t['backgroundTaskType']}>
        <Task task={t} />
      </Grid>
    ))

    return (
      <div>
        <h2>Tasks</h2>
        <Grid container spacing={1} justifyContent="space-between" alignItems="center">
          {tasks}
        </Grid>
      </div>
    )
  }

  if (state.loading) return <div />

  return (
    <NarrowLayout>
      <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>
          <h1>Settings</h1>
        </Grid>

        <Grid size={6}>
          <h3>Device</h3>
          <p>{getGuid()}</p>
        </Grid>

        <Grid size={6}>
          <Button color='primary' variant='contained' fullWidth={true} onClick={onRouteChange}>
            Edit Manual
          </Button>
        </Grid>

        <Grid size={4}>{renderEvents()}</Grid>
        <Grid size={8}>{renderTasks()}</Grid>
      </Grid>
    </NarrowLayout>
  )
}

export default withAuth(Setting)
