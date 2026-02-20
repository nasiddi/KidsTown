import React, { useEffect, useState } from 'react'
import Grid from '@mui/material/Grid'
import { getEventId, LargeButton, StyledTextField } from '../components/Common'
import StarIcon from '@mui/icons-material/Star'
import { postPhoneNumbers, postWithJsonResult } from '../helpers/BackendClient'

export function CheckInPhoneNumbers(props) {
  const [adults, setAdults] = useState([])

  useEffect(() => {
    async function load() {
      const phoneNumbers = await loadPhoneNumbers(props.attendanceIds)
      setAdults(phoneNumbers)
    }

    load().then()
  }, [props.attendanceIds])

  async function loadPhoneNumbers(attendanceIds) {
    if (attendanceIds.length === 0) {
      return []
    }

    const json = await postWithJsonResult('people/adults', attendanceIds)
    json.forEach((a) => (a.isEdit = false))

    return json
  }

  async function onPrimaryContactChange(event) {
    const id = getEventId(event)
    const primary = adults.find((a) => a.personId === id)

    if (primary.isPrimaryContact) {
      primary.isPrimaryContact = false
    } else {
      adults.forEach((a) => (a.isPrimaryContact = false))
      primary.isPrimaryContact = true
    }

    setAdults([...adults])
    await postPhoneNumbers(adults, false)
  }

  async function onSave(event) {
    const eventId = getEventId(event)
    adults.find((f) => f.personId === eventId).isEdit = false

    setAdults([...adults])
    await postPhoneNumbers(adults, true)
  }

  function onEdit(event) {
    const eventId = getEventId(event)
    adults.find((f) => f.personId === eventId).isEdit = true

    setAdults([...adults])
  }

  const onPhoneNumberChange = (e) => {
    const eventId = getEventId(e)
    const adult = adults.find((a) => a.personId === eventId)
    adult.phoneNumber = e.target.value

    setAdults([...adults])
  }

  function renderPhoneNumberEditButton(id, phoneNumber, isEdit) {
    if (isEdit) {
      return (
        <Grid size={{ md: 2, xs: 4 }}>
          <LargeButton id={id} name="Save" color="success" onClick={onSave} />
        </Grid>
      )
    }

    return (
      <Grid size={{ md: 2, xs: 4 }}>
        <LargeButton id={id} name="Edit" color="primary" onClick={onEdit} />
      </Grid>
    )
  }

  function renderPhoneNumber(id, phoneNumber, isEdit) {
    if (isEdit) {
      return (
        <Grid size={{ md: 3, xs: 8 }}>
          <StyledTextField
            id={id}
            label="PhoneNumber"
            variant="outlined"
            value={phoneNumber}
            fullWidth={true}
            onChange={onPhoneNumberChange}
          />
        </Grid>
      )
    }

    return (
      <Grid size={{ md: 3, xs: 8 }}>
        <h4
          style={{
            justifyContent: 'center',
            height: '100%',
            margin: 0,
          }}
        >
          {phoneNumber}
        </h4>
      </Grid>
    )
  }

  function getPhoneNumberIsEdit(personId) {
    const adult = adults.find((a) => a.personId === personId)

    return adult.isEdit
  }

  const entries = adults.map((a) => (
    <Grid size={12} key={a.personId}>
      <Grid container spacing={3} justifyContent="space-between" alignItems="center">
        <Grid size={{ md: 1, xs: 3 }}>
          <LargeButton
            id={a.personId}
            name={
              <span id={a.personId}>
                <StarIcon />
              </span>
            }
            color='primary'
            isOutline={!a.isPrimaryContact}
            onClick={onPrimaryContactChange}
          />
        </Grid>
        <Grid size={{ md: 6, xs: 9 }}>
          <h4
            style={{
              justifyContent: 'center',
              height: '100%',
              margin: 0,
            }}
          >
            {`${a.firstName} ${a.lastName}`}
          </h4>
        </Grid>
        {renderPhoneNumber(a.personId, a.phoneNumber, getPhoneNumberIsEdit(a.personId))}
        {renderPhoneNumberEditButton(a.personId, a.phoneNumber, getPhoneNumberIsEdit(a.personId))}
      </Grid>
    </Grid>
  ))

  return (
    <Grid container spacing={3} justifyContent="space-between" alignItems="center">
      {entries}
    </Grid>
  )
}
