'use client'
import React, {useEffect, useState} from 'react'
import {Table, TableBody, TableCell, TableHead, TableRow,} from '@mui/material'
import Grid from '@mui/material/Grid2';
import {
    getLastSunday,
    getSelectedEventFromStorage,
    getSelectedOptionsFromStorage,
    getStringFromSession,
    TableHeadCell,
    TableTotalCell,
} from '../components/Common'
import {fetchLocationGroups, postWithJsonResult} from "@/app/helpers/BackendClient";

export default function OverviewHeadCount() {
    const [state, setState] = useState({
        headCounts: [],
        locationGroups: []
    })

    async function loadData() {
        const locationGroups = state.locationGroups.length === 0
            ? await fetchLocationGroups()
            : state.locationGroups
        const headCounts = await fetchData()
        setState({
            ...state,
            loading: false,
            locationGroups: locationGroups,
            headCounts: headCounts,
        })
    }

    const sum = (a, b) => {
        return a + b
    }

    useEffect(() => {
        const interval = setInterval(() => {
            loadData().then()
        }, 2000)

        return () => clearInterval(interval)
    })

    function renderCounts() {
        if (state.loading) {
            return <div/>
        }

        const headCounts = state.headCounts

        let totalCount = <TableRow/>
        if (headCounts.length !== 1) {
            totalCount = (
                <TableRow key="Total">
                    <TableTotalCell>
                        <strong>Total</strong>
                    </TableTotalCell>
                    <TableTotalCell>
                        <strong>{getTotalCount(false)}</strong>
                    </TableTotalCell>
                    <TableTotalCell>
                        <strong>{getTotalCount(true)}</strong>
                    </TableTotalCell>
                </TableRow>
            )
        }

        return (
            <Table size="small">
                <TableHead>
                    <TableRow>
                        <TableHeadCell>
                            <strong>Location</strong>
                        </TableHeadCell>
                        <TableHeadCell>
                            <strong>Kinder</strong>
                        </TableHeadCell>
                        <TableHeadCell>
                            <strong>Betreuer</strong>
                        </TableHeadCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {headCounts.map((row) => (
                        <TableRow key={row['location']}>
                            <TableCell> {row['location']}</TableCell>
                            <TableCell>{getCount(row, false)}</TableCell>
                            <TableCell>{getCount(row, true)}</TableCell>
                        </TableRow>
                    ))}
                    {totalCount}
                </TableBody>
            </Table>
        )
    }

    function getCount(headCounts, isVolunteer) {
        if (isVolunteer) {
            return headCounts['volunteersCount']
        }

        return headCounts['kidsCount']
    }

    function getTotalCount(isVolunteer) {
        if (isVolunteer) {
            const locationCount = state.headCounts.map(
                (h) => h['volunteersCount']
            )

            return locationCount.reduce(sum, 0)
        }

        const locationCount = state.headCounts.map((h) => h['kidsCount'])

        return locationCount.reduce(sum, 0)
    }

    async function fetchData() {
        const selectedOptions = getSelectedOptionsFromStorage(
            'overviewLocationGroups',
            []
        )

        if (selectedOptions.length === 0) {
            return []
        }

        return await postWithJsonResult(
            `overview/event/${await getSelectedEventFromStorage()}/attendees/headcounts?date=${getStringFromSession(
                'overviewDate',
                getLastSunday().toISOString()
            )}`,
            selectedOptions.map((l) => l.value))
    }

    if (state.loading) {
        return <div/>
    }

    return (
        <div>
            <Grid
                container
                spacing={3}
                justifyContent="space-between"
                alignItems="flex-start"
            >
                <Grid size={12}>
                    <h3/>
                </Grid>
                <Grid size={12}>
                    <h3>Checked-In</h3>
                </Grid>
                <Grid size={12}>
                    {renderCounts()}
                </Grid>
            </Grid>
        </div>
    )
}
