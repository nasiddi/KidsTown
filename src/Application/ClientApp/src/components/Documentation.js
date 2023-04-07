import React from 'react'
import { Grid } from '@mui/material'
import { FullText, TextImageSplit, Title } from './DocElements'
import { NarrowLayout } from './Layout'

function Docs() {
	return (
		<div>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
				alignItems="flex-start"
				id={'checkinsAppTabName'}
			>
				<Title text="Anleitung Checkin KidsTown" size={1} />
				<Title
					text="CheckIns App (Label Stationen)"
					size={2}
					id={'CheckInsApp'}
				/>
				<Title text="Stationstypen" size={3} id={'Stationstypen'} />
				<FullText
					paragraphs={[
						{
							text:
								"Das App bietet verschiedene Typen von Station. Wir brauchen die 'Self' Station und die 'Manned' Station.\n" +
								"                        Bei 'Self' Stationen können Eltern die Label für ihre Kinder selber drucken. 'Manned' Stationen müssen aus Datenschutzgründen immer von Mitarbeitern betreut werden.",
						},
					]}
				/>
				<Title
					text="Self Station Ablauf"
					size={4}
					id={'SelfStationAblauf'}
				/>
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
				<Title
					text="Manned Station Ablauf"
					size={4}
					id={'MannedStationAblauf'}
				/>
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
					title={'Personen erfassen'}
					fileName={'manned_add.png'}
					paragraphs={[
						{
							icon: 'Action',
							text: "Wenn eine Person nicht gefunden wird, muss man im Suchfeld den ganzen Namen eingeben und dann auf 'Add Them' (1) klicken",
						},
						{
							icon: 'Info',
							text: "Auf der nächsten Seite hat man zwei Optionen: Einen neuen Haushalt erstellen oder einem bestehenden Haushalt hinzufügen. Das weitere Vorgehen für 'zu bestehendem hinzufügen' entspricht dem Abschnitt 'Haushalt editieren'",
						},
						{
							icon: 'Action',
							text: 'Familie Mustermann will mal bei uns reinschnuppern. Und bringen Sohn Timo ins Kidsland. In diesem Fall gibt man im Suchfeld seinen ganzen Namen ein.',
						},
					]}
				/>
				<TextImageSplit
					title={'Gast erfassen'}
					fileName={'household_visitor.png'}
					paragraphs={[
						{
							icon: 'Action',
							text: "Nach 'Gast KidsTown' suchen, oder schneller 'ga ki' und 'The KidsTown HouseHold' auswählen.",
						},
						{
							icon: 'Action',
							text: "Im neuen Fenster werden die Angaben des Kindes und eine Kontaktperson eingetragen. Und dann weiter mit 'Next' unten rechts.",
						},
						{
							icon: 'Action',
							text: "Nun kann oben rechts auf der Haushaltübersicht 'Add Visitor' auswählt werden.",
						},
						{
							icon: 'Action',
							text: 'Nun ist der Gast auf der Haushaltsübersicht erschienen und kann gleich wie die anderen Kinder einchecken.',
						},
						{
							icon: 'Warning',
							text: "Die automatisch ausgwählte Location ist meist nicht korrekt und kann, wie in 'Location anpassen' beschrieben, angepasst werden.",
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
			</Grid>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
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
				<TextImageSplit
					fileName={'checkin_locationchange.png'}
					title={'Jemand in eine andere Location einchecken'}
					paragraphs={[
						{
							icon: 'Info',
							text: 'Theo ist eigentlich in der KidsChurch, wird aber heute seine kleine Schwester zu den Füchsli begleiten. Nach abscannen des Barcodes erscheint die Meldung dass niemand gefunden wurde, sowie einen gelben Knopf "Suche ohne Location Filter".',
						},
						{
							icon: 'Action',
							text: 'Beim drücken auf diesen Knopf, werden alle Locations durchsucht. Es werden alle Kinder mit dem Securitycode angezeigt, die noch nicht eingecheckt sind.',
						},
						{
							icon: 'Action',
							text: 'In diesem Fall wollen wir Theo einchecken, also wählen wir Theo aus. Somit verschwinden alle weiteren Kinder von der Anzeige.',
						},
						{
							icon: 'Info',
							text: "Unter Theo's Knopf ist jetzt eine Auswahlliste mit Locations erschienen. Es werden nur Locations angezeigt, die auch im Filter für die Suche ausgewählt sind.",
						},
						{
							icon: 'Action',
							text: 'Nach auswählen der richtigen Location erscheint der grüne CheckIn Knopf. Nochmals überprüfen, ob die Einstellung stimmt und einchecken',
						},
						{
							icon: 'Warning',
							text: 'Theo ist an diesem Tag in der Overview und in der Statistik bei den Füchsli gelistet. Es ist nur möglich, jemanden in eine andere Location einzuchecken, wenn die Person noch nicht eingecheckt ist.',
						},
					]}
				/>
			</Grid>
		</div>
	)
}

export default function Documentation() {
	return (
		<NarrowLayout>
			<Docs />
		</NarrowLayout>
	)
}
