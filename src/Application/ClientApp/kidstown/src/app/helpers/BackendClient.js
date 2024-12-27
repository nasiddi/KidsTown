'use client'
import {getGuid, getSelectedEventFromStorage} from '../components/Common'

const backendHost = 'http://localhost:5000';

export async function fetchLocationGroups() {
    return await getJson('configuration/location-groups')
}

export async function postSecurityCode(
    securityCode,
    locationIds,
    isFastCheckout,
    checkType,
    filterLocations
) {
    const body = {
        securityCode: securityCode,
        eventId: await getSelectedEventFromStorage(),
        selectedLocationGroupIds: locationIds,
        isFastCheckInOut: isFastCheckout ?? false,
        checkType: checkType,
        attendanceIds: [],
        filterLocations: filterLocations,
        guid: getGuid(),
    }

    return await postWithJsonResult('checkinout/people', body)
}

export async function fetchLocations() {
    return await getJson(`configuration/events/${await getSelectedEventFromStorage()}/location-groups/locations`)
        .then((j) => {
            return j
        })
}

export async function postCheckInOut(candidates, checkType, securityCode) {
    const body = {
        checkType: checkType,
        checkInOutCandidates: candidates,
        securityCode: securityCode,
    }

    return await postWithJsonResult('checkinout/manual', body)
}

export async function postPhoneNumbers(adults, numberChanged) {
    await post(
        `people/adults/update?updatePhoneNumber=${numberChanged}`,
        adults
    ).then()
}

export async function postImages(files, elementId) {
    const request = {
        method: 'POST',
        headers: {
            us: localStorage.getItem('username'),
            pw: localStorage.getItem('passwordHash'),
        },
    }

    const formData = new FormData()

    for (let i = 0; i < files.length; i++) {
        formData.append(`file${i}`, files[i])
    }

    request.body = formData

    const res = await fetch(backendHost + `/documentation/${elementId}/image-upload/`, request)

    return await res.json()
}

export async function fetchDocumentation() {
    const scanStation = await get('documentation/1')
    const labelStation = await get('documentation/2')

    return [...(await scanStation.json()), ...(await labelStation.json())]
}


export async function post(endpoint, body) {
    return await fetch(backendHost + "/" + (endpoint.startsWith('/') ? endpoint.slice(1) : endpoint), {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            us: localStorage.getItem('username'),
            pw: localStorage.getItem('passwordHash'),
        },
        body: JSON.stringify(body),
    })
}

export async function get(endpoint) {
    return await fetch(backendHost + "/" + (endpoint.startsWith('/') ? endpoint.slice(1) : endpoint), {
        method: 'GET',
        headers: {
            us: localStorage.getItem('username'),
            pw: localStorage.getItem('passwordHash'),
        },
    })
}

export async function getJson(endpoint) {
    return (await get(endpoint)).json()
}

export async function postWithJsonResult(endpoint, body) {
    return (await post(endpoint, body)).json()
}
