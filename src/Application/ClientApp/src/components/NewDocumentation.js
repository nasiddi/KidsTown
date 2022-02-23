import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import classnames from 'classnames'
import $ from 'jquery'
import { FullText, TextImageSplit, Title } from './DocElements'
import {
	Dropdown,
	DropdownItem,
	DropdownMenu,
	DropdownToggle,
	Nav,
	Navbar,
} from 'reactstrap'

const checkinsAppTabNameDefault = {
	tab: 'dropdownCheckInsApp',
	key: 'checkinsAppTabName',
	name: 'CheckIns App (Label Stationen)',
}
const kidsTownAppTabNameDefault = {
	tab: 'dropdownKidsTownApp',
	key: 'kidsTownAppTabName',
	name: 'Kidstown WebApp (Scan Stationen)',
}

export class NewDocumentation extends Component {
	static displayName = NewDocumentation.name
	repeat

	constructor(props) {
		super(props)

		this.state = {
			loading: false,
			dropdownCheckInsApp: false,
			dropdownKidsTownApp: false,
			checkinsAppTabName: checkinsAppTabNameDefault.name,
			kidsTownAppTabName: kidsTownAppTabNameDefault.name,
			activeTab: 'dropdownCheckInsApp',
			currentHash: '',
			content: [],
		}

		if (props.location.hash.length > 1) {
			window.location.href = props.location.hash
		}

		this.handleDropdownEvent = this.handleDropdownEvent.bind(this)
		this.toggleDropdown = this.toggleDropdown.bind(this)
		this.handleClick = this.handleClick.bind(this)
	}

	handleDropdownEvent(event) {
		const dropdown = event.target.name
		this.toggleDropdown(dropdown)
	}

	toggleDropdown(dropdown) {
		const previous = this.state[dropdown]
		this.setState({ [dropdown]: !previous })
	}

	handleClick = (e) => {
		e.preventDefault()
		window.location.href = `#${e.target.name}`
		const activeTab = e.target.offsetParent.id
		const activeName =
			activeTab === checkinsAppTabNameDefault.tab
				? checkinsAppTabNameDefault
				: kidsTownAppTabNameDefault

		const defaultName =
			activeTab !== checkinsAppTabNameDefault.tab
				? checkinsAppTabNameDefault
				: kidsTownAppTabNameDefault

		this.setState({
			activeTab: activeTab,
			[activeName.key]: `${activeName.name} → ${e.target.innerText}`,
			[defaultName.key]: defaultName.name,
		})
		this.toggleDropdown(activeTab)
	}

	async componentDidMount() {
		const that = this
		$(window).scroll(function () {
			$('.hash').each(function () {
				const top = window.pageYOffset
				const distance = top - $(this).offset().top
				const hash = $(this).attr('href')

				if (
					distance < 50 &&
					distance > 0 &&
					hash !== undefined &&
					that.state.currentHash !== hash
				) {
					const activeTab = this.parentElement.parentElement.id
					const activeName =
						activeTab === checkinsAppTabNameDefault.key
							? checkinsAppTabNameDefault
							: kidsTownAppTabNameDefault

					const defaultName =
						activeTab !== checkinsAppTabNameDefault.key
							? checkinsAppTabNameDefault
							: kidsTownAppTabNameDefault

					const subTitle = this.parentElement.children[0].innerText
					that.setState({
						currentHash: hash,
						activeTab: activeName.tab,
						[activeName.key]:
							subTitle === activeName.name
								? activeName.name
								: `${activeName.name} → ${subTitle}`,
						[defaultName.key]: defaultName.name,
					})

					const ref = `#${hash}`
					if (window.location.href !== ref) {
						window.location.href = `#${hash}`
					}
				}
			})
		})

		const response = await fetch('documentation')
		const doc = await response.json()
		this.setState({ content: doc })
	}

	renderElement(element) {
		if (element.isTitle) {
			return <Title text={element.title.text} size={element.title.size} />
		}

		const entry = element.entry

		const sortedParagraphs = _.sortBy(entry.paragraphs, [
			function (p) {
				return p.position
			},
		])

		if (entry.fileName !== null) {
			return (
				<TextImageSplit
					title={entry.title !== null ? entry.title : ''}
					fileName={entry.fileName}
					paragraphs={sortedParagraphs}
				/>
			)
		}

		return <FullText paragraphs={sortedParagraphs} />
	}

	render() {
		if (this.state.loading) {
			return <div />
		}

		let checkInsApp = []

		if (this.state.content.length > 0) {
			checkInsApp = _(this.state.content)
				.filter((c) => c.entry !== null || c.title !== null)
				.filter((c) => c.tabId === 1)
				.sortBy((e) => e.position)
				.value()

			console.log(checkInsApp)
		}

		return (
			<div>
				<Navbar
					className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3 sticky-nav stick-offset"
					light
					sticky="top"
				>
					<Nav tabs>
						<Dropdown
							nav
							isOpen={this.state.dropdownCheckInsApp}
							toggle={this.handleDropdownEvent}
						>
							<DropdownToggle
								nav
								caret
								name={'dropdownCheckInsApp'}
								className={classnames({
									active:
										this.state.activeTab ===
										'dropdownCheckInsApp',
								})}
							>
								{this.state.checkinsAppTabName}
							</DropdownToggle>
							<DropdownMenu id={'dropdownCheckInsApp'}>
								<DropdownItem
									name={'CheckInsApp'}
									onClick={this.handleClick}
								>
									Allgemein
								</DropdownItem>
								<DropdownItem
									name={'Stationstypen'}
									onClick={this.handleClick}
								>
									Stationstypen
								</DropdownItem>
								<DropdownItem
									name={'SelfStationAblauf'}
									onClick={this.handleClick}
								>
									Self Station Ablauf
								</DropdownItem>
								<DropdownItem
									name={'MannedStationAblauf'}
									onClick={this.handleClick}
								>
									Self Station Ablauf
								</DropdownItem>
							</DropdownMenu>
						</Dropdown>
						<Dropdown
							nav
							isOpen={this.state.dropdownKidsTownApp}
							toggle={this.handleDropdownEvent}
						>
							<DropdownToggle
								nav
								caret
								name={'dropdownKidsTownApp'}
								className={classnames({
									active:
										this.state.activeTab ===
										'dropdownKidsTownApp',
								})}
							>
								{this.state.kidsTownAppTabName}
							</DropdownToggle>
							<DropdownMenu id={'dropdownKidsTownApp'}>
								<DropdownItem
									name={'KidsTownApp'}
									onClick={this.handleClick}
								>
									Allgemein
								</DropdownItem>
								<DropdownItem
									name={'Navigation'}
									onClick={this.handleClick}
								>
									Navigation
								</DropdownItem>
								<DropdownItem
									name={'KidsTownLogin'}
									onClick={this.handleClick}
								>
									Login
								</DropdownItem>
								<DropdownItem
									name={'CheckInOut'}
									onClick={this.handleClick}
								>
									CheckIn/Out
								</DropdownItem>
							</DropdownMenu>
						</Dropdown>
					</Nav>
				</Navbar>
				<Grid
					container
					spacing={3}
					justify="space-between"
					alignItems="flex-start"
					id={'checkinsAppTabName'}
				>
					{checkInsApp.map(this.renderElement)}
				</Grid>
				<Grid
					container
					spacing={3}
					justify="space-between"
					alignItems="flex-start"
					id={'kidsTownAppTabName'}
				>
					<Title
						text={'Kidstown WebApp (Scan Stationen)'}
						size={2}
						id={'KidsTownApp'}
					/>
					<Title text={'Navigation'} size={3} id={'Navigation'} />
					<TextImageSplit
						fileName={'navbar.png'}
						paragraphs={[
							{
								text: 'Über die Navigations Liste sind folgende Seiten erreichbar:',
							},
							{
								text: 'CheckIn/Out: Hier können Kinder ein und ausgecheckt werden.',
							},
							{
								text: 'Overview: Zeigt an, welche Personen an einem bestimmten Tag eingecheckt sind.',
							},
							{
								text: 'Statistic: Zeigt alle Daten, an denen ein CheckIn stattgefunden hat, an, wie viele Personen eingecheckt waren.',
							},
							{
								text: 'Anleitung: Hier ist diese Anleitung zu finden.',
							},
						]}
					/>
					<Title text={'Login'} size={3} id={'KidsTownLogin'} />
					<FullText
						paragraphs={[
							{
								text: 'Für alle Funktionen, ausser für der Anleitung, muss man sich einloggen. Dafür wird ein GvC Microsoft Account benötigt, das heisst, alle GvC Staff Members können sich einloggen. Falls niemand vor Ort ist, kann man kidstown@gvc.ch verwenden. Die Hauptleitung teilt in diesem Fall das Passwort der verantwortlichen Person mit.',
							},
						]}
					/>
					<Title text={'CheckIn/Out'} size={3} id={'CheckInOut'} />
					<TextImageSplit
						title={'Übersicht'}
						fileName={'checkin_multi.png'}
						paragraphs={[
							{
								icon: 'Info',
								text: 'Hier kann ein Barcode abgescannt oder eingetippt werden. Es werden alle Kinder angezeigt, die unter dem SecurityCode und den eingestellten Locations gefunden werden.',
							},
						]}
					/>
					<Title text={'Funktionen'} size={4} />
					<TextImageSplit
						title={'CheckIn/Out, Locations & FastCheckIn'}
						fileName={'checkin_default.png'}
						paragraphs={[
							{
								icon: 'Info',
								text: 'Im Vorfeld muss die Scan Station richtig konfiguriert werden. Ganz links kann eigestellt werden, ob man Kinder ein oder auschecken will und ganz rechts kann man auswählen, für welche Locations man Kinder ein oder auschecken will. In diesem Fall ist die Location Füchsli ausgewählt.',
							},
							{
								icon: 'Info',
								text: 'Weiter gibt es zwei Checkboxen. Wenn FastCheckIn aktiv ist, muss man nicht noch zusätzlich bestätigen, wenn das System nur ein Kind findet und es wird automatisch eingecheckt.',
							},
						]}
					/>
					<TextImageSplit
						title={'SingleCheckIn'}
						fileName={'checkin_single.png'}
						paragraphs={[
							{
								icon: 'Info',
								text: 'Wenn SingleCheckIn deaktiviert ist, gibt es unter den Namen ein CheckIn Knopf. Ist der Knopf farbig ausgefüllt, so wird das Kind eingecheckt, wenn man auf CheckIn tippt. Wenn man auf ein Kind tippt, ist der Knopf nur noch umrandet. Jetzt wird es nicht mit eingecheckt.',
							},
							{
								icon: 'Info',
								text: 'Wenn SingleCheckIn aktiv ist, gibt es den CheckIn Knopf nicht. In diesem Fall wird ein Kind eingecheckt, sobald man auf den Namen tippt.',
							},
						]}
					/>
					<Title text={'Szenarien'} size={4} id={'Szenarien'} />
					<TextImageSplit
						title={'FastCheckIn/Out & darf nicht alleine gehen'}
						fileName={'checkin_fast.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: 'Levi ist das einzige Kind der Familie im Alter der Füchsli, deshalb kommt nach abscannen des Codes sofort die Meldung, dass Levi eingecheckt wurde.',
							},
							{
								icon: 'Action',
								text: 'Beim CheckOut siehts jedoch anders aus, denn Levi muss abgeholt werden. Eine entsprechende Meldung wird angezeigt und Levis Knopf ist gelb eingefärbt. Der Checkout muss noch manuel bestätigt werden.',
							},
						]}
					/>
					<TextImageSplit
						title={'MultiCheckIn/Out'}
						fileName={'checkin_multi_singular.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: "Lea und Noah werden zusammen eingecheckt. Nach abscannen des Codes werden beide Namen angezeigt und mit einem Klick auf 'CheckIn' wird eine entsprechende Erfolgsmeldung angezeigt.",
							},
							{
								icon: 'Action',
								text: 'Noah muss leider früher abgeholt werden. Deshalb wird nun nach abscannen des Codes Lea deselektiert, indem man ihren Namen antippt. Wenn man nun auf CheckOut klickt, wird nur Noah ausgecheckt.',
							},
						]}
					/>
					<TextImageSplit
						fileName={'checkin_kab.png'}
						title={'SingleCheckOut & keine Abholberechtigung'}
						paragraphs={[
							{
								icon: 'Info',
								text: 'Jemand will Noah abholen. Jedoch ist bei im zusätzliche vorsicht geboten, da es Personen gibt, die ihn nicht abholen dürfen. Deshalb wird nach Abscannen des Codes eine entsprechende Meldung angezeigt und Noahs Knopf ist rot.',
							},
							{
								icon: 'Action',
								text: 'Nachdem verifiziert wurde, das die Person abholberechtigt ist, wird Levi durch deine Klick auf seinen Namen audgecheckt.',
							},
						]}
					/>
				</Grid>
			</div>
		)
	}
}
