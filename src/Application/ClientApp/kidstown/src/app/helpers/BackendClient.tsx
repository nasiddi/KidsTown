import { getGuid } from '../components/CommonHelpers'

const backendHost = 'api'

export async function fetchLocationGroups(): Promise<LocationGroupOption[]> {
  return await getJson('configuration/location-groups')
}

export async function postSecurityCode(
  securityCode: string,
  locationIds: number[],
  isFastCheckout: boolean,
  checkType: string,
  filterLocations: boolean,
): Promise<SecurityCodeResponse> {
  const body = {
    securityCode: securityCode,
    eventId: await getSelectedEventFromStorage(),
    selectedLocationGroupIds: locationIds,
    isFastCheckInOut: isFastCheckout ?? false,
    checkType: checkType,
    attendanceIds: [] as number[],
    filterLocations: filterLocations,
    guid: getGuid(),
  }

  return await postWithJsonResult('checkinout/people', body)
}

export async function fetchLocations(): Promise<LocationsByGroup[]> {
  return await getJson(
    `configuration/events/${await getSelectedEventFromStorage()}/location-groups/locations`,
  ).then((j: LocationsByGroup[]) => {
    return j
  })
}

export async function postCheckInOut(
  candidates: CheckInOutCandidate[],
  checkType: string,
  securityCode: string,
): Promise<CheckInOutResponse> {
  const body = {
    checkType: checkType,
    checkInOutCandidates: candidates,
    securityCode: securityCode,
  }

  return await postWithJsonResult('checkinout/manual', body)
}

export async function postPhoneNumbers(adults: Adult[], numberChanged: boolean): Promise<void> {
  await post(`people/adults/update?updatePhoneNumber=${numberChanged}`, adults).then()
}

export async function fetchDocumentation(): Promise<DocElement[]> {
  const scanStation = await get('documentation/1')
  const labelStation = await get('documentation/2')

  return [...(await scanStation.json()), ...(await labelStation.json())]
}

export async function post(endpoint: string, body: unknown): Promise<Response> {
  return await fetch(
    backendHost + '/' + (endpoint.startsWith('/') ? endpoint.slice(1) : endpoint),
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        us: localStorage.getItem('username') ?? '',
        pw: localStorage.getItem('passwordHash') ?? '',
      },
      body: JSON.stringify(body),
    },
  )
}

export async function get(endpoint: string): Promise<Response> {
  return await fetch(
    backendHost + '/' + (endpoint.startsWith('/') ? endpoint.slice(1) : endpoint),
    {
      method: 'GET',
      headers: {
        us: localStorage.getItem('username') ?? '',
        pw: localStorage.getItem('passwordHash') ?? '',
      },
    },
  )
}

export async function getJson(endpoint: string): Promise<any> {
  return (await get(endpoint)).json()
}

export async function postWithJsonResult(endpoint: string, body: unknown): Promise<any> {
  return (await post(endpoint, body)).json()
}

export async function getSelectedEventFromStorage(): Promise<number> {
  const s = localStorage.getItem('selectedEvent')

  if (s === null) {
    const selectedEvent = await getJson('configuration/events/default').then(
      (j: any) => j['eventId'],
    )
    localStorage.setItem('selectedEvent', selectedEvent)
    return selectedEvent
  } else {
    return JSON.parse(s)
  }
}

// --- Shared types ---

export interface LocationGroupOption {
  value: number
  label: string
}

export interface LocationOption {
  value: number
  label: string
  isSelected?: boolean
}

export interface LocationsByGroup {
  groupId: number
  options: LocationOption[]
}

export interface CheckInOutCandidate {
  attendanceId: number
  name: string
  isSelected: boolean
  mayLeaveAlone: boolean
  hasPeopleWithoutPickupPermission: boolean
  locationId?: number
}

export interface SecurityCodeResponse {
  alertLevel: string
  text: string
  checkInOutCandidates: CheckInOutCandidate[]
  attendanceIds?: number[]
  successfulFastCheckout?: boolean
  filteredSearchUnsuccessful?: boolean
}

export interface CheckInOutResponse {
  alertLevel: string
  text: string
  attendanceIds?: number[]
}

export interface Adult {
  personId: number
  firstName: string
  lastName: string
  phoneNumber: string
  isPrimaryContact: boolean
  isEdit: boolean
}

export interface AlertInfo {
  text: string
  level: string | number
}

export interface DocElement {
  id: number
  previousId: number
  sectionId: number
  title?: { text: string; size: number }
  paragraphs: DocParagraph[]
  images: DocImage[]
}

export interface DocParagraph {
  id: number
  previousId: number
  text: string
  icon: string
}

export interface DocImage {
  id: number
  previousId: number
  fileId: string
}

export interface HeadCountRow {
  location: string
  kidsCount: number
  volunteersCount: number
}

export interface AttendeeRow {
  attendanceId: number
  firstName: string
  lastName: string
  checkState: string
  securityCode: string
  adults: AdultInfo[]
}

export interface AdultInfo {
  firstName: string
  lastName: string
  phoneNumber: string
  isPrimaryContact: boolean
}

export interface LocationAttendees {
  location: string
  kids: AttendeeRow[]
  volunteers: AttendeeRow[]
}

export interface StatisticRow {
  date: string
  regularCount: number
  guestCount: number
  volunteerCount: number
  preCheckInOnlyCount: number
  noCheckOutCount: number
}

export interface EventInfo {
  eventId: number
  name: string
}

export interface BackgroundTask {
  backgroundTaskType: string
  isActive: boolean
  taskRunsSuccessfully: boolean
  lastExecution: string
  successCount: number
  currentFailCount: number
  interval: number
  logFrequency: number
}
