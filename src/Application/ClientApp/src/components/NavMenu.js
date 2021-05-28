/* eslint-disable react/jsx-no-bind */
import React, { Component } from 'react'
import {
	Collapse,
	Container,
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

export class NavMenu extends Component {
	static displayName = NavMenu.name

	constructor(props) {
		super(props)

		this.toggleNavbar = this.toggleNavbar.bind(this)
		this.state = {
			collapsed: true,
			location: location,
			checkInOutClass: inactive,
			guestCheckInClass: inactive,
			overViewClass: inactive,
			statisticClass: inactive,
			manualClass: inactive,
		}

		this.setTextStyles = this.setTextStyles.bind(this)
		this.checkInActive = this.checkInActive.bind(this)
		this.guestCheckInActive = this.guestCheckInActive.bind(this)
		this.overviewActive = this.overviewActive.bind(this)
		this.statisticActive = this.statisticActive.bind(this)
		this.manualActive = this.manualActive.bind(this)
	}

	toggleNavbar() {
		this.setState({
			collapsed: !this.state.collapsed,
		})
	}

	componentDidMount() {
		this.initTextStyles()
	}

	initTextStyles() {
		this.setTextStyles(this.state.location.pathname)
		const checkInOutClass =
			this.state.location.pathname === '/checkin' ? active : inactive

		const guestCheckInClass =
			this.state.location.pathname === '/guest' ? active : inactive

		const overViewClass =
			this.state.location.pathname === '/overview' ? active : inactive

		const statisticClass =
			this.state.location.pathname === '/statistic' ? active : inactive

		const manualClass =
			this.state.location.pathname === '/' ? active : inactive

		this.setState({
			checkInOutClass: checkInOutClass,
			guestCheckInClass: guestCheckInClass,
			overViewClass: overViewClass,
			statisticClass: statisticClass,
			manualClass: manualClass,
		})
	}

	setTextStyles(path) {
		const checkInOutClass = path === '/checkin' ? active : inactive

		const guestCheckInClass = path === '/guest' ? active : inactive

		const overViewClass = path === '/overview' ? active : inactive

		const statisticClass = path === '/statistic' ? active : inactive

		const manualClass = path === '/' ? active : inactive

		this.setState({
			checkInOutClass: checkInOutClass,
			guestCheckInClass: guestCheckInClass,
			overViewClass: overViewClass,
			statisticClass: statisticClass,
			manualClass: manualClass,
		})
	}

	checkInActive() {
		this.setTextStyles('/checkin')
	}

	guestCheckInActive() {
		this.setTextStyles('/guest')
	}

	overviewActive() {
		this.setTextStyles('/overview')
	}

	statisticActive() {
		this.setTextStyles('/statistic')
	}

	manualActive() {
		this.setTextStyles('/')
	}

	render() {
		return (
			<Navbar
				className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3 sticky-nav"
				light
				sticky="top"
			>
				<Container>
					<NavbarBrand tag={Link} to="/">
						Kidstown
					</NavbarBrand>
					<NavbarToggler
						onClick={this.toggleNavbar}
						className="mr-2"
					/>
					<Collapse
						className="d-sm-inline-flex flex-sm-row-reverse"
						isOpen={!this.state.collapsed}
						navbar
					>
						<ul className="navbar-nav flex-grow">
							<Nav
								className={this.state.checkInOutClass}
								to={'/checkin'}
								onClick={this.checkInActive}
								label={'CheckIn/Out'}
							/>
							<Nav
								className={this.state.overViewClass}
								to="/overview"
								onClick={this.overviewActive}
								label={'Overview'}
							/>
							<Nav
								className={this.state.statisticClass}
								to="/statistic"
								onClick={this.statisticActive}
								label={'Statistic'}
							/>
							<Nav
								className={this.state.manualClass}
								to="/"
								onClick={this.manualActive}
								label={'Anleitung'}
							/>
						</ul>
					</Collapse>
				</Container>
			</Navbar>
		)
	}
}
