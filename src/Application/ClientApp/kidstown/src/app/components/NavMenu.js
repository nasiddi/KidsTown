'use client'
import React, {useEffect, useState} from 'react'
import {AppBar, Button, Toolbar, Typography} from '@mui/material'
import {usePathname, useRouter} from 'next/navigation'

const active = 'text-dark font-weight-bold'
const inactive = 'text-dark'

export function NavMenu() {
    const router = useRouter()
    const pathname = usePathname()

    const [state, setState] = useState({
        checkInOutClass: inactive,
        guestCheckInClass: inactive,
        overViewClass: inactive,
        statisticClass: inactive,
        manualClass: inactive,
    })

    useEffect(() => {
        initTextStyles()
    }, [pathname]) // Update styles when pathname changes

    function initTextStyles() {
        setState({
            checkInOutClass: pathname === '/checkin' ? active : inactive,
            guestCheckInClass: pathname === '/guest' ? active : inactive,
            overViewClass: pathname === '/overview' ? active : inactive,
            statisticClass: pathname === '/statistic' ? active : inactive,
            manualClass: pathname === '/' ? active : inactive,
        })
    }

    function isSelected(location) {
        if (location.length === 0) {
            return pathname === '/'
        }
        return pathname.toLowerCase().includes(location.toLowerCase())
    }

    function onRouteChange(event) {
        const path = `/${event.currentTarget.name ?? ''}`
        router.push(path) // Use router.push for client-side navigation
    }

    function getNavButton(route, label) {
        const selected = isSelected(route)

        return (
            <Button
                style={{width: 115}}
                disableElevation
                color={'white'}
                name={route}
                variant={selected ? 'outlined' : 'contained'}
                onClick={onRouteChange}
            >
                {label ?? route}
            </Button>
        )
    }

    return (
        <AppBar>
            <Toolbar variant="dense">
                <Typography variant="h6" component="div" sx={{flexGrow: 1}}>
                    KidsTown
                </Typography>
                {getNavButton('checkin', 'CheckIn/Out')}
                {getNavButton('overview', 'Overview')}
                {getNavButton('statistic', 'Statistic')}
                {getNavButton('', 'Anleitung')}
            </Toolbar>
        </AppBar>
    )
}
