import React, { Component } from 'react'
import { Grid } from '@material-ui/core'
import { FullText, SplitText, TextImageSplit, Title } from './DocElements'

export class Documentation extends Component {
	static displayName = Documentation.name

	constructor(props) {
		super(props)

		this.state = {
			loading: false,
		}
	}

	async componentDidMount() {}

	render() {
		if (this.state.loading) {
			return <div />
		}

		return (
			<div>
				<Grid
					container
					spacing={3}
					justify="space-between"
					alignItems="flex-start"
				>
					<Title text="Anleitung Checkin KidsTown" size={1} />
					<Title text="CheckIns App (Label Stationen)" size={2} />
					<Title text="Stationstypen" size={3} />
					<FullText
						paragraphs={[
							{
								text:
									"Das App bietet verschiedene Typen von Station. Wir brauchen die 'Self' Station und die 'Manned' Station.\n" +
									"Bei 'Self' Stationen können Eltern die Label für ihre Kinder selber drucken. 'Manned' Stationen müssen aus Datenschutzgründen immer von Mitarbeitern betreut werden.",
							},
						]}
					/>
					<Title text="Self Station Ablauf" size={4} />
					<TextImageSplit
						title={'Startseite'}
						fileName="self_start.png"
						paragraphs={[
							{
								icon: 'Action',
								text: "Paul Muster möchte seine vier Kinder einchecken, also gibt er dafür seine Handynummer ein und tippt auf 'Search!'.",
							},
							{
								icon: 'Action',
								text: 'Oder Paul Muster hat bereits einen BarCode für sich registriert und tippt deshalb auf den BarCode und hält seinen BarCode in die Kamera.',
							},
							{
								icon: 'Info',
								text: 'Das ist die Startseite. Hier kann man über eine beliebige Telefonnummer, die in diesem Haushalt erfasst ist, ober über einen hinterlegten BarCode zum Haushalt gelangen. Telefonnummern können nur über Manned Stationen hinzugefügt oder bearbeitet werden.',
							},
						]}
					/>
					<TextImageSplit
						title={'Haushaltsübersicht'}
						fileName={'self_household.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: "Paul hat die vier Kinder ausgewählt, wenn alles passt, tippt Paul unten rechts auf 'CheckIn 4 People'. Jetzt werden die Labels gedruckt und die Station zeigt wieder die Startseite an.",
							},
							{
								icon: 'Action',
								text: "Falls man den falschen Haushalt geöffnet hat kommt man über 'Start Over' oben rechts zurück zur Startseite",
							},
							{
								icon: 'Info',
								text: 'Hier ist der ganze Haushalt von jung nach alt sortiert zu sehen. Sobald eine Person ausgewählt ist, wir nach Alter oder Klasse die Location gesetzt und unter jedem Namen angezeigt.',
							},
							{
								icon: 'Info',
								text: "Die Buchstaben der Avatars, z.B. 'nm' bei Noah Muster sind klein geschrieben bei Kindern und gross bei Erwachsenen.",
							},
						]}
					/>
					<TextImageSplit
						title="Location ändern"
						fileName={'self_location.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: 'Bei Lea muss Paul noch die Location korrigieren. Dafür tippt er bei Lea auf den Stift (1).',
							},
							{
								icon: 'Action',
								text: 'Jetzt tippt er neben der Location nochmals auf den Stift (2), wählt die richtige Location aus (3) und dann zwei mal  auf done (4) und (5). Jetzt ist Lea richtig eingeteilt.',
							},
							{
								icon: 'Info',
								text: 'Mit der Self Station können Personen nur in Locations eingeteilt werden, die dem vorgegebenen Alter und Klasse entsprechen. Für andere Locations muss Paul bei der Manned Station vorbei.',
							},
							{
								icon: 'Info',
								text: 'Alle Kinder unter drei Jahren werden erstmals bei den Häsli eingeteilt und müssen einmalig umgeteilt werden, wenn sie zu den Schöfli kommen.',
							},
						]}
					/>
					<TextImageSplit
						title="Zusätzliche Labels drucken"
						fileName={'self_label.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: 'Lea braucht ein zusätzliches Namenslabel, also tippt Paul auf den Stift bei ihrem Namen (1) zählt das Label aus (2) und tippt auf done (3).',
							},
							{
								icon: 'Info',
								text: 'Hier kann man ein und/oder zwei zusätzliche Nameslabel für die Kinder auswählen. Wenn man beide selektiert, erhält man drei Labels. Zudem kann ein (weiteres) SecurityLabel drucken. Gedruckt werden die Labels zusammen mit den restlichen, wenn der CheckIn abgeschlossen wird.',
							},
						]}
					/>
					<Title text="Manned Station Ablauf" size={4} />
					<TextImageSplit
						title={'Startseite'}
						fileName={'manned_start.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: 'Paul Muster kommt mit seinen Kindern zur Manned Station. Die Betreungsperson sucht nach Pauls Namen. Damit sie nicht den ganzen Namen eintippen muss, kann sie einfach nach den ersten zwei Buchstaben von Vor- und Nachname suchen. Passende Vorschläge werden automatisch angezeigt und aus der Liste kann Paul ausgewählt werden.',
							},
							{
								icon: 'Info',
								text: 'Es kann nach einem beliebigen Familienmitglied gesucht werden. Bei allen wird im nächsten Schritt immer der ganze Haushalt angezeigt',
							},
						]}
					/>
					<TextImageSplit
						title={'Haushaltsübersicht'}
						fileName={'manned_household.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: "Paul hat die vier Kinder ausgewählt, wenn alles passt, tippt Paul unten rechts auf 'CheckIn 4 People'. Jetzt werden die Labels gedruckt und die Station zeigt wieder die Startseite an.",
							},
							{
								icon: 'Action',
								text: "Falls man den falschen Haushalt geöffnet hat kommt man über 'Start Over' oben rechts zurück zur Startseite",
							},
							{
								icon: 'Info',
								text: 'Hier ist der ganze Haushalt von jung nach alt sortiert zu sehen. Sobald eine Person ausgewählt ist, wir nach Alter oder Klasse die Location gesetzt und unter jedem Namen angezeigt.',
							},
							{
								icon: 'Info',
								text: "Die Buchstaben der Avatars, z.B. 'nm' bei Noah Muster sind klein geschrieben bei Kindern und gross bei Erwachsenen.",
							},
						]}
					/>
					<TextImageSplit
						title="Location ändern"
						fileName={'manned_location.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: 'Bei Lea muss noch die Location korrigiert werden. Dafür tippt man bei Lea auf den Stift (1).',
							},
							{
								icon: 'Action',
								text: 'Dann gleich nochmals neben der Location auf den Stift (2), wählt die richtige Location aus (3) und dann zwei mal auf done (4) und (5).',
							},
							{
								icon: 'Info',
								text: 'Bei Manned Stationen ist des möglich unten auf alle Locations zu tippen und ein Kind in eine beliebige Location einzuchecken. Das sollte aber grundsätzlich unterlassen werden, da es darauf hindeutet, das etwas an den Daten nicht stimmt, z.B. Geburtsdatum oder Klasse fehlt, und das muss nachhaltig über Haushalt editieren korrigiert werden. Sonst muss man das beim nächsten mal wieder manuel anpassen.',
							},
							{
								icon: 'Info',
								text: 'Alle Kinder unter drei Jahren werden erstmals bei den Häsli eingeteilt und müssen einmalig umgeteilt werden, wenn sie zu den Schöfli kommen.',
							},
						]}
					/>
					<TextImageSplit
						title="Zusätzliche Labels drucken"
						fileName={'self_label.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: 'Lea braucht ein zusätzliches Namenslabel, also tippt Paul auf den Stift bei ihrem Namen (1) zählt das Label aus (2) und tippt auf done (3).',
							},
							{
								icon: 'Info',
								text: 'Hier kann man ein und/oder zwei zusätzliche Nameslabel für die Kinder auswählen. Wenn man beide selektiert, erhält man drei Labels. Zudem kann ein (weiteres) SecurityLabel drucken. Gedruckt werden die Labels zusammen mit den restlichen, wenn der CheckIn abgeschlossen wird.',
							},
						]}
					/>
					<TextImageSplit
						title={'Haushalt editieren & BarCode erfassen'}
						fileName={'household_edit_simple.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: "Als ersten Schritt muss man die Person auswählen, die man editieren will (1) oder falls es die Person noch nicht gibt, mit 'Add New' (5) eine neue erfassen. Hier kann man diverse Felder editieren wie z.B. (2) die Telefonnummer. Auch die Medizinischen Information, das Geburtsdatum oder die Klasse (Grade) können hier gesetz werden. Zu kann hier ein BarCode für die erleichterte Suche erfasst werden (3).",
							},
							{
								icon: 'Action',
								text: "Wenn alle Änderungen gemacht sind, muss man sie noch speichern, hierfür oder rechts auf 'Save Household' tippen (6), oder falls man die Änderungen verwerfen möchte, auf 'GoBack'",
							},
							{
								icon: 'Info',
								text: 'Welche Felder direkt angezeigt werden variert, um alle zu sehen unten auf den Pfeil klicken (4).',
							},
							{
								icon: 'Warning',
								text: 'Beim editieren ist Vorsicht geboten, da diese Änderungen für unsere ganze Personendatenbank gilt.',
							},
						]}
					/>
					<TextImageSplit
						title={'Personen erfassen (Mitglieder & Gäste)'}
						fileName={'manned_add.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: "Wenn eine Person nicht gefunden wird, muss man im Suchfeld den ganzen Namen eingeben und dann auf 'Add Them' (1) klicken",
							},
							{
								icon: 'Info',
								text: "Auf der nächsten Seite hat man drei Optionen: Einen neuen Haushalt erstellen (2), einem Bestehenden Haushalt hinzufügen (3) oder als Gast erfassen (4). Das weitere vorgehen für (2) und (3) entspricht dem Abschnitt 'Haushalt editieren'",
							},
							{
								icon: 'Action',
								text: 'Familie Mustermann will mal bei uns reinschnuppern. Und bringen Sohn Timo ins Kidsland. In diesem Fall gibt man im Suchfeld seinen ganzen Namen ein.',
							},
						]}
					/>
					<TextImageSplit
						title={'Gast in einem Haushalt eintragen'}
						fileName={'household_visitor.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: "Oben rechts auf der Haushaltsübersicht muss man auf den Knopf 'Add Visitor tippen.",
							},
							{
								icon: 'Action',
								text: "Im neuen Fenster werden die Angaben des Kindes eingetragen. Und dann weiter mit 'Next' unten rechts.",
							},
							{
								icon: 'Action',
								text: 'Nun ist der Gast auf der Haushaltsübersicht erschienen und man kann wie gewohnt die Location anpassen, oder zusätzliche Labels auswählen.',
							},
							{
								icon: 'Info',
								text: 'Auf diese Art sollten Gastkinder erfasst werden, die mit einer Familie mitgekommen sind. Wenn ein Kind unabhängig gekommen ist, sollte es über die Startseite erfasst werden. Wenn das Gastkind mit anderen Kindern im Haushalt eingecheckt wird, erhalten alle den gleichen SecurityCode.',
							},
						]}
					/>
					<Title text="Manned Station Haushalt editieren" size={4} />
					<TextImageSplit
						fileName={'household_edit.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: 'Wenn man die Haushaltsübersicht geöffnet hat, kann man oben beim Haushalt auf den Stift tippen um ihn zu editieren',
							},
						]}
					/>

					<SplitText
						title="Fehlende Abschnitte"
						paragraphs={[{ text: 'Manned Station, Settings' }]}
					/>
					<Title text={'Kidstown WebApp (Scan Stationen)'} size={2} />
					<Title text={'Navigation'} size={3} />
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
					<Title text={'Login'} size={3} />
					<FullText
						paragraphs={[
							{
								text: 'Für alle Funktionen, ausser für der Anleitung, muss man sich einloggen. Dafür wird ein GvC Microsoft Account benötigt, das heisst, alle GvC Staff Members können sich einloggen. Falls niemand vor Ort ist, kann man kidstown@gvc.ch verwenden. Die Hauptleitung teilt in diesem Fall das Passwort der verantwortlichen Person mit.',
							},
						]}
					/>
					<Title text={'CheckIn/Out'} size={3} />
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
					<Title text={'Szenarien'} size={4} />
					<TextImageSplit
						title={'FastCheckIn/Out & darf nicht alleine gehen'}
						fileName={'checkin_fast.png'}
						paragraphs={[
							{
								icon: 'Action',
								text: 'Levi ist das einzige Kind der Familie im Alter der Füchsli, deshalb kommt nach abscannen des Codes sofort die Meldung, dass Levi engecheckt wurde.',
							},
							{
								icon: 'Action',
								text: 'Beim CheckOut siehts jedoch anders aus, denn Levi muss abgeholt werden. Eine entsprechende Meldung wird angezeigt und Levis Kopf ist gelb eingefärbt. Der Checkout muss noch manuel bestätigt werden.',
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
