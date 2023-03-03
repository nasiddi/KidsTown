import React, { useEffect, useState } from 'react'
import { AppBar, Button, Toolbar, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'

const active = 'text-dark font-weight-bold'
const inactive = 'text-dark'

export function NavMenu() {
	const [state, setState] = useState({
		collapsed: true,
		location: location,
		checkInOutClass: inactive,
		guestCheckInClass: inactive,
		overViewClass: inactive,
		statisticClass: inactive,
		manualClass: inactive,
	})

	useEffect(() => {
		initTextStyles()
	}, [])

	const navigate = useNavigate()

	const onRouteChange = (event) => {
		const path = `/${event.currentTarget.name ?? ''}`
		navigate(path)
	}

	function initTextStyles() {
		setTextStyles(state.location.pathname)
		const checkInOutClass =
			state.location.pathname === '/checkin' ? active : inactive

		const guestCheckInClass =
			state.location.pathname === '/guest' ? active : inactive

		const overViewClass =
			state.location.pathname === '/overview' ? active : inactive

		const statisticClass =
			state.location.pathname === '/statistic' ? active : inactive

		const manualClass = state.location.pathname === '/' ? active : inactive

		setState({
			...state,
			checkInOutClass: checkInOutClass,
			guestCheckInClass: guestCheckInClass,
			overViewClass: overViewClass,
			statisticClass: statisticClass,
			manualClass: manualClass,
		})
	}

	function setTextStyles(path) {
		const checkInOutClass = path === '/checkin' ? active : inactive

		const guestCheckInClass = path === '/guest' ? active : inactive

		const overViewClass = path === '/overview' ? active : inactive

		const statisticClass = path === '/statistic' ? active : inactive

		const manualClass = path === '/' ? active : inactive

		setState({
			...state,
			checkInOutClass: checkInOutClass,
			guestCheckInClass: guestCheckInClass,
			overViewClass: overViewClass,
			statisticClass: statisticClass,
			manualClass: manualClass,
		})
	}

	function isSelected(location) {
		if (location.length === 0) {
			return window.location.href.slice(-1) === '/'
		}

		return window.location.href
			.toLowerCase()
			.includes(location.toLowerCase())
	}

	function getNavButton(route, label) {
		const selected = isSelected(route)

		return (
			<Button
				style={{ width: 115 }}
				disableElevation
				color={selected ? 'secondary' : 'primary'}
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
				<Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
					KidsTown
				</Typography>
				{getNavButton('Checkin', 'CheckIn/Out')}
				{getNavButton('Overview')}
				{getNavButton('Statistic')}
				{getNavButton('', 'Anleitung')}
			</Toolbar>
		</AppBar>
	)
}
