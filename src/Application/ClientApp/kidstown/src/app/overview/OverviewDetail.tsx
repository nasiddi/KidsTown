import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import PhoneIphoneIcon from '@mui/icons-material/PhoneIphone'
import StarIcon from '@mui/icons-material/Star'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Chip,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import type { ChipOwnProps } from '@mui/material'
import Grid from '@mui/material/Grid'
import { styled } from '@mui/material/styles'
import React, { useEffect, useState } from 'react'

import { HtmlTooltip, TableHeadCell } from '../components/Common'
import {
  getLastSunday,
  getSelectedFromSession,
  getSelectedOptionsFromStorage,
  getStringFromSession,
} from '../components/CommonHelpers'
import type {
  AdultInfo,
  AttendeeRow,
  LocationAttendees,
  LocationGroupOption,
} from '../helpers/BackendClient'
import { getSelectedEventFromStorage, post } from '../helpers/BackendClient'

const StyledTableCell = styled(TableCell)({
  height: 25,
})

const StyledTableHeadCell = styled(TableHeadCell)({
  height: 30,
})

interface OverviewDetailState {
  attendees: LocationAttendees[]
  loading: boolean
  error: string | null
}

export default function OverviewDetail() {
  const [state, setState] = useState<OverviewDetailState>({
    attendees: [],
    loading: true,
    error: null,
  })

  async function fetchData() {
    const selectedOptionsFromStorage = getSelectedOptionsFromStorage<LocationGroupOption[]>(
      'overviewLocationGroups',
      [],
    )

    if (selectedOptionsFromStorage.length === 0) {
      setState((prev) => ({ ...prev, loading: false }))
      return
    }

    await post(
      `overview/event/${await getSelectedEventFromStorage()}/attendees?date=${getStringFromSession(
        'overviewDate',
        getLastSunday().toISOString(),
      )}`,
      selectedOptionsFromStorage.map((l) => l.value),
    )
      .then((r) => r.json())
      .then((j: LocationAttendees[]) => {
        setState((prev) => ({ ...prev, attendees: j, loading: false }))
      })
      .catch(() => {
        setState((prev) => ({ ...prev, loading: false, error: 'Failed to load.' }))
      })
  }

  useEffect(() => {
    async function run() {
      await fetchData()
    }

    run().then()
    const interval = setInterval(() => {
      fetchData().then()
    }, 30000)

    return () => clearInterval(interval)
  }, [])

  function renderDetails() {
    return (
      <Grid container spacing={3}>
        {state.attendees.map((attendees) => (
          <Grid size={12} key={attendees.location}>
            <Accordion className="overview-accordion" defaultExpanded>
              <AccordionSummary
                expandIcon={<ExpandMoreIcon />}
                aria-controls="panel1a-content"
                id="panel1a-header"
              >
                <Typography variant="subtitle1" component="h3" fontWeight="bold">
                  {attendees.location}
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={1}>
                  <Grid size={{ xs: 12, md: 8 }}>{renderKidsTable(attendees.kids)}</Grid>
                  <Grid size={{ xs: 12, md: 4 }}>{renderVolunteerTable(attendees.volunteers)}</Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>
          </Grid>
        ))}
      </Grid>
    )
  }

  function renderKidsTable(kids: AttendeeRow[]) {
    const selectedState = getSelectedFromSession<string[]>('selectedOverviewStates', [
      'PreCheckedIn',
      'CheckedIn',
      'CheckedOut',
    ])
    const filteredKids = kids.filter((k) => selectedState.includes(k.checkState))

    return (
      <div>
        <Typography variant="subtitle2" component="h4">
          Kinder
        </Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <StyledTableHeadCell>
                <strong>First Name</strong>
              </StyledTableHeadCell>
              <StyledTableHeadCell>
                <strong>Last Name</strong>
              </StyledTableHeadCell>
              <StyledTableHeadCell>
                <div style={{ textAlign: 'center' }}>
                  <PhoneIphoneIcon fontSize={'small'} />
                </div>
              </StyledTableHeadCell>
              <StyledTableHeadCell style={{ width: 100 }}>
                <strong>Status</strong>
              </StyledTableHeadCell>
              <StyledTableHeadCell>
                <strong>SecurityCode</strong>
              </StyledTableHeadCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredKids.map((row) => (
              <TableRow key={row.attendanceId}>
                <StyledTableCell>{row.firstName}</StyledTableCell>
                <StyledTableCell>{row.lastName}</StyledTableCell>
                <StyledTableCell>
                  <div id={getToolTipTarget(row)} style={{ textAlign: 'center' }}>
                    {getTooltip(row)}
                  </div>
                </StyledTableCell>
                <StyledTableCell style={{ width: 100 }}>
                  {getStateBadge(row.checkState)}
                </StyledTableCell>
                <StyledTableCell>{row.securityCode}</StyledTableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    )
  }

  function getTooltip(row: AttendeeRow) {
    if (row.adults.length === 0) {
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

  function getToolTipTarget(row: AttendeeRow): string {
    return `${row.firstName.replace(/\s+/g, '')}${row.lastName.replace(
      /\s+/g,
      '',
    )}${row.attendanceId}`
  }

  function renderVolunteerTable(volunteers: AttendeeRow[]) {
    return (
      <div>
        <Typography variant="subtitle2" component="h4">
          Betreuer
        </Typography>
        <Table size="small">
          <TableHead sx={{ padding: '-50px' }}>
            <TableRow>
              <StyledTableHeadCell>
                <strong>First Name</strong>
              </StyledTableHeadCell>
              <StyledTableHeadCell>
                <strong>Last Name</strong>
              </StyledTableHeadCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {volunteers.map((row) => (
              <TableRow key={row.attendanceId}>
                <StyledTableCell>{row.firstName}</StyledTableCell>
                <StyledTableCell>{row.lastName}</StyledTableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    )
  }

  function getStateBadge(checkState: string) {
    let color: ChipOwnProps['color'] = 'default'

    if (checkState === 'PreCheckedIn') {
      color = 'info'
    }

    if (checkState === 'CheckedIn') {
      color = 'success'
    }

    if (checkState === 'CheckedOut') {
      color = 'primary'
    }

    return (
      <Chip
        color={color}
        label={checkState}
        size="small"
        style={{ width: '-webkit-fill-available' }}
      />
    )
  }

  function getAdultInfos(row: AttendeeRow) {
    return (
      <span>
        {row.adults.map((a: AdultInfo) => {
          const primary = a.isPrimaryContact ? <StarIcon /> : <div />

          return (
            <Typography key={`${a.firstName} ${a.lastName} ${a.phoneNumber}`}>
              {primary} {`${a.firstName} ${a.lastName} ${a.phoneNumber}`}
              <br />
            </Typography>
          )
        })}
      </span>
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

  if (state.attendees.length === 0) {
    return (
      <Typography color="text.secondary" sx={{ p: 2 }}>
        Select location groups in the filter to see attendees.
      </Typography>
    )
  }

  return (
    <div>
      <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>{renderDetails()}</Grid>
      </Grid>
    </div>
  )
}
