import { getGuid, getSelectedEventFromStorage } from '../components/Common'
const _ = require('lodash')

const postJson = {
	method: 'POST',
	headers: {
		'Content-Type': 'application/json',
	},
}

function getPostRequest() {
	return _.cloneDeep(postJson)
}

export async function fetchLocationGroups() {
	const response = await fetch('configuration/location-groups')

	return await response.json()
}

export async function fetchDefaultEvent() {
	return await fetch('configuration/events/default')
		.then((r) => r.json())
		.then((j) => j['eventId'])
}

export async function postSecurityCode(
	securityCode,
	locationIds,
	isFastCheckout,
	checkType
) {
	const request = getPostRequest()
	request.body = JSON.stringify({
		securityCode: securityCode,
		eventId: await getSelectedEventFromStorage(),
		selectedLocationIds: locationIds,
		isFastCheckInOut: isFastCheckout ?? false,
		checkType: checkType,
		attendanceIds: [],
		guid: getGuid(),
	})

	return await fetch('checkinout/people', request).then((r) => r.json())
}

export async function postCheckInOut(candidates, checkType) {
	const request = getPostRequest()
	request.body = JSON.stringify({
		checkType: checkType,
		checkInOutCandidates: candidates,
	})

	return await fetch('checkinout/manual', request).then((r) => r.json())
}

export async function postUndo(lastActionAttendanceIds, checkType) {
	const request = getPostRequest()
	request.body = JSON.stringify(lastActionAttendanceIds)

	return await fetch(`checkinout/undo/${checkType}`, request).then((r) =>
		r.json()
	)
}

export async function fetchParentPhoneNumbers(lastActionAttendanceIds) {
	const request = getPostRequest()
	request.body = JSON.stringify(lastActionAttendanceIds)

	return await fetch('people/adults', request).then((r) => r.json())
}

export async function postPhoneNumbers(adults, numberChanged) {
	const request = getPostRequest()
	request.body = JSON.stringify(adults)

	await fetch(
		`people/adults/update?updatePhoneNumber=${numberChanged}`,
		request
	).then()
}
