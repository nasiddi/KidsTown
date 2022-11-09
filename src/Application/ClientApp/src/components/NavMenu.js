/* eslint-disable react/jsx-no-bind */
import React, { useEffect, useState } from 'react'
import {
	Collapse,
	Navbar,
	NavbarBrand,
	NavbarToggler,
	NavItem,
	NavLink,
} from 'reactstrap'
import { Link } from 'react-router-dom'
import './NavMenu.css'

const active = 'text-dark font-weight-bold'
const inactive = 'text-dark'

function Nav(props) {
	return (
		<NavItem>
			<NavLink
				tag={Link}
				className={props['className']}
				to={props['to']}
				onClick={props['onClick']}
			>
				{props['label']}
			</NavLink>
		</NavItem>
	)
}

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

	function toggleNavbar() {
		setState({
			...state,
			collapsed: !state.collapsed,
		})
	}

	useEffect(() => {
		initTextStyles()
	}, [])

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

	function checkInActive() {
		setTextStyles('/checkin')
	}

	function guestCheckInActive() {
		setTextStyles('/guest')
	}

	function overviewActive() {
		setTextStyles('/overview')
	}

	function statisticActive() {
		setTextStyles('/statistic')
	}

	function manualActive() {
		setTextStyles('/')
	}

	return (
		<Navbar
			className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3 sticky-nav"
			light
			sticky="top"
			container={'md'}
		>
			<NavbarBrand tag={Link} to="/">
				Kidstown
			</NavbarBrand>
			<NavbarToggler onClick={toggleNavbar} className="mr-2" />
			<Collapse
				className="d-sm-inline-flex flex-sm-row-reverse"
				isOpen={!state.collapsed}
				navbar
			>
				<ul className="navbar-nav flex-grow">
					<Nav
						className={state.checkInOutClass}
						to={'/checkin'}
						onClick={checkInActive}
						label={'CheckIn/Out'}
					/>
					<Nav
						className={state.overViewClass}
						to="/overview"
						onClick={overviewActive}
						label={'Overview'}
					/>
					<Nav
						className={state.statisticClass}
						to="/statistic"
						onClick={statisticActive}
						label={'Statistic'}
					/>
					<Nav
						className={state.manualClass}
						to="/"
						onClick={manualActive}
						label={'Anleitung'}
					/>
				</ul>
			</Collapse>
		</Navbar>
	)
}
