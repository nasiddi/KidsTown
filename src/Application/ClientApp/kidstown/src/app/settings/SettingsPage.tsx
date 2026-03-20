import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Checkbox,
  Chip,
  CircularProgress,
  Divider,
  FormControlLabel,
  Typography,
} from '@mui/material'
import type { ChipOwnProps } from '@mui/material'
import Grid from '@mui/material/Grid'
import { useEffect, useState } from 'react'
import type React from 'react'
import { useNavigate } from 'react-router-dom'

import { getGuid } from '../components/CommonHelpers'
import { NarrowLayout } from '../components/Layout'
import withAuth from '../components/withAuth'
import type { BackgroundTask, EventInfo } from '../helpers/BackendClient'
import { get, getSelectedEventFromStorage } from '../helpers/BackendClient'

interface BoolBadgeProps {
  color: ChipOwnProps['color']
  label: string
}

function BoolBadge({ color, label }: BoolBadgeProps) {
  return <Chip color={color} label={label} />
}

interface TaskProps {
  task: BackgroundTask
}

function Task({ task }: TaskProps) {
  function GetBadge(
    isActive: boolean,
    trueColor: ChipOwnProps['color'],
    trueLabel: string,
    falseColor: ChipOwnProps['color'],
    falseLabel: string,
  ) {
    if (isActive === true) {
      return <BoolBadge color={trueColor} label={trueLabel} />
    }
    return <BoolBadge color={falseColor} label={falseLabel} />
  }

  const failedCount: React.ReactNode[] = []

  if (!task.taskRunsSuccessfully) {
    failedCount.push(
      <Grid size={12} key="fc-hr-1">
        <Divider sx={{ my: 1 }} />
      </Grid>,
    )
    failedCount.push(
      <Grid size={{ md: 6, xs: 12 }} key="fc-label">
        <Typography component="span" variant="body2" fontWeight="bold">
          Failed Executions
        </Typography>
      </Grid>,
    )
    failedCount.push(
      <Grid size={{ md: 6, xs: 12 }} key="fc-value">
        {task.currentFailCount}
      </Grid>,
    )
  }

  return (
    <Card>
      <CardHeader title={task.backgroundTaskType} />
      <CardContent>
        <Grid container spacing={1} justifyContent="space-between" alignItems="center">
          <Grid size={{ md: 6, xs: 12 }}>
            {GetBadge(task.isActive, 'primary', 'active', 'secondary', 'inactive')}{' '}
            {GetBadge(task.taskRunsSuccessfully, 'success', 'success', 'error', 'failed')}
          </Grid>

          <Grid size={12}>
            <Divider sx={{ my: 1 }} />
          </Grid>

          <Grid size={{ md: 6, xs: 12 }}>
            <Typography component="span" variant="body2" fontWeight="bold">
              Last Execution
            </Typography>
          </Grid>
          <Grid size={{ md: 6, xs: 12 }}>
            {new Date(task.lastExecution).toLocaleString('de-CH')}
          </Grid>

          <Grid size={12}>
            <Divider sx={{ my: 1 }} />
          </Grid>

          <Grid size={{ md: 6, xs: 12 }}>
            <Typography component="span" variant="body2" fontWeight="bold">
              Successful Executions
            </Typography>
          </Grid>
          <Grid size={{ md: 6, xs: 12 }}>{task.successCount}</Grid>

          {failedCount}

          <Grid size={12}>
            <Divider sx={{ my: 1 }} />
          </Grid>

          <Grid size={{ md: 6, xs: 12 }}>
            <Typography component="span" variant="body2" fontWeight="bold">
              Interval
            </Typography>
          </Grid>
          <Grid size={{ md: 6, xs: 12 }}>{task.interval} milliseconds</Grid>

          <Grid size={12}>
            <Divider sx={{ my: 1 }} />
          </Grid>

          <Grid size={{ md: 6, xs: 12 }}>
            <Typography component="span" variant="body2" fontWeight="bold">
              Log Frequency
            </Typography>
          </Grid>
          <Grid size={{ md: 6, xs: 12 }}>Every {task.logFrequency} executions</Grid>
        </Grid>
      </CardContent>
    </Card>
  )
}

interface SettingState {
  events: EventInfo[]
  tasks: BackgroundTask[]
  selectedEvent: number
  loading: boolean
  error: string | null
}

function Setting() {
  const navigate = useNavigate()

  const [state, setState] = useState<SettingState>({
    events: [],
    tasks: [],
    selectedEvent: 0,
    loading: true,
    error: null,
  })

  async function fetchAvailableEvents(): Promise<EventInfo[]> {
    const response = await get('configuration/events')
    return await response.json()
  }

  async function fetchTasks(): Promise<BackgroundTask[]> {
    const response = await get('configuration/tasks')
    return await response.json()
  }

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
      setState((prev) => ({ ...prev, loading: false, error: 'Failed to load.' }))
    })
  }, [])

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      const selected = state.events.find((e) => e.name === event.target.name)
      if (!selected) return

      localStorage.setItem('selectedEvent', String(selected.eventId))
      localStorage.setItem('statisticLocations', JSON.stringify([]))
      setState((prev) => ({ ...prev, selectedEvent: selected.eventId }))
    }
  }

  const onRouteChange = () => {
    navigate('/settings/documentation')
  }

  function renderEvents() {
    const events = state.events.map((e) => (
      <Grid size={12} key={e.eventId}>
        <div className="event">
          <FormControlLabel
            control={
              <Checkbox
                name={e.name}
                color="primary"
                onChange={handleChange}
                checked={state.selectedEvent === e.eventId}
              />
            }
            label={''}
            labelPlacement="end"
          />
          <a
            href={`https://check-ins.planningcenteronline.com/events/${e.eventId}`}
            target="_blank"
            rel="noopener noreferrer"
          >
            {e.name}
          </a>
        </div>
      </Grid>
    ))

    return (
      <div>
        <Typography variant="h6" component="h2">
          Events
        </Typography>
        <Grid container spacing={0} justifyContent="space-between" alignItems="center">
          {events}
        </Grid>
      </div>
    )
  }

  function renderTasks() {
    if (state.loading) {
      return (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      )
    }

    const tasks = state.tasks.map((t) => (
      <Grid size={12} key={t.backgroundTaskType}>
        <Task task={t} />
      </Grid>
    ))

    return (
      <div>
        <Typography variant="h6" component="h2">
          Tasks
        </Typography>
        <Grid container spacing={1} justifyContent="space-between" alignItems="center">
          {tasks}
        </Grid>
      </div>
    )
  }

  if (state.loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (state.error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        {state.error}
      </Alert>
    )
  }

  return (
    <NarrowLayout>
      <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>
          <Typography variant="h5" component="h1">
            Settings
          </Typography>
        </Grid>

        <Grid size={6}>
          <Typography variant="subtitle1" component="h3" fontWeight="bold">
            Device
          </Typography>
          <Typography variant="body1">{getGuid()}</Typography>
        </Grid>

        <Grid size={6}>
          <Button color="primary" variant="contained" fullWidth={true} onClick={onRouteChange}>
            Edit Manual
          </Button>
        </Grid>

        <Grid size={4}>{renderEvents()}</Grid>
        <Grid size={8}>{renderTasks()}</Grid>
      </Grid>
    </NarrowLayout>
  )
}

const SettingsPage = withAuth(Setting)
export default SettingsPage
