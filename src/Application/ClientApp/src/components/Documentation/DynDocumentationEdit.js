import React from 'react'
import { Button, Grid } from '@mui/material'
import { useEffect, useState } from 'react'
import { DynDocEditElement } from './DynDocEditElement'
import { withAuth } from '../../auth/MsalAuthProvider'
import { NarrowLayout } from '../Layout'
import { StyledButton } from '../Common'
import {
	fetchDocumentation,
	onDeleteSubElement,
	onDownSubElement,
	onUpSubElement,
	sortDocElements,
} from './DynDocHelpers'
import { postImages, saveDocumentation } from '../../helpers/BackendClient'
import { CheckInAlert } from '../CheckIn/CheckInAlert'
import { Title } from './DynDocElement'

function DynDocsEdit() {
	const [state, setState] = useState({
		documentation: [],
		alert: {
			text: '',
			level: 'success',
		},
		loading: true,
		isSaving: false,
	})

	useEffect(() => {
		async function load() {
			setState({
				...state,
				documentation: await fetchDocumentation(),
				loading: false,
			})
		}

		load().then()
	}, [])

	async function saveDoc() {
		setState({
			...state,
			alert: {
				text: '',
				level: 'success',
			},
			isSaving: true,
		})

		const status = await saveDocumentation(state.documentation)

		if (status === 200) {
			const response = await fetchDocumentation()

			setState({
				...state,
				documentation: response,
				alert: {
					text: 'Speichern erfolgreich',
					level: 'success',
				},
				isSaving: false,
			})

			return
		}

		let text = 'Etwas ist schiefgegangen'

		if (status === 409) {
			text = 'Eine neuere Version der Anleitung wurde bereits gespeichert'
		}

		setState({
			...state,
			alert: {
				text: text,
				level: 'error',
			},
		})
	}

	function onEdit(event) {
		const element = state.documentation.find(
			(e) => e.id.toString() === event.currentTarget.id
		)

		element.isEdit = true

		setState({ ...state, documentation: state.documentation })
	}

	function onEditOff(event) {
		const element = state.documentation.find(
			(e) => e.id.toString() === event.currentTarget.id
		)

		element.isEdit = false

		setState({ ...state, documentation: state.documentation })
	}

	function onUp(event) {
		const element = state.documentation.find(
			(e) => e.id.toString() === event.currentTarget.id
		)

		const previous = state.documentation.find(
			(e) => e.id === element.previousId
		)

		const following = state.documentation.find(
			(e) => e.previousId === element.id
		)

		const upperPreviousId = previous.previousId

		previous.previousId = element.id
		element.previousId = upperPreviousId

		if (following) {
			following.previousId = previous.id
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onUpParagraph(event) {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)
		onUpSubElement(paragraphs, event.currentTarget.id)
		setState({ ...state, documentation: state.documentation })
	}

	function onDown(event) {
		const element = state.documentation.find(
			(e) => e.id.toString() === event.currentTarget.id
		)

		const previous = state.documentation.find(
			(e) => e.id === element.previousId
		)

		const following = state.documentation.find(
			(e) => e.previousId === element.id
		)

		const followingFollowing = state.documentation.find(
			(e) => e.previousId === following.id
		)

		following.previousId = previous?.id || 0
		element.previousId = following.id

		if (followingFollowing) {
			followingFollowing.previousId = element.id
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onElementDelete(event) {
		const id = parseInt(event.currentTarget.id, 10)

		const element = state.documentation.find((e) => e.id === id)

		const previous = state.documentation.find(
			(e) => e.id === element.previousId
		)

		const following = state.documentation.find(
			(e) => e.previousId === element.id
		)

		const index = state.documentation.findIndex((e) => e.id === element.id)

		state.documentation.splice(index, 1)

		if (following) {
			following.previousId = previous?.id || 0
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onDownParagraph(event) {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)
		onDownSubElement(paragraphs, event.currentTarget.id)
		setState({ ...state, documentation: state.documentation })
	}

	function onTitleChange(event) {
		const element = state.documentation.find(
			(e) => e.id.toString() === event.currentTarget.id
		)

		element.title.text = event.currentTarget.value

		setState({ ...state, documentation: state.documentation })
	}

	function onParagraphChange(event) {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)

		const paragraph = paragraphs.find(
			(e) => e.id?.toString() === event.currentTarget.id
		)

		paragraph.text = event.currentTarget.value

		setState({ ...state, documentation: state.documentation })
	}

	function onTitleSizeChange(event) {
		const element = state.documentation.find(
			(e) => e.id.toString() === event.currentTarget.id
		)

		element.title.size = parseInt(event.currentTarget.name, 10)

		setState({ ...state, documentation: state.documentation })
	}

	function onParagraphIconChange(event) {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)

		const paragraph = paragraphs.find(
			(e) => e.id?.toString() === event.currentTarget.id
		)

		if (paragraph.icon === event.currentTarget.name) {
			paragraph.icon = null
		} else {
			paragraph.icon = event.currentTarget.name
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onParagraphDelete(event) {
		const id = parseInt(event.currentTarget.id, 10)

		const paragraphs = state.documentation.find((e) =>
			e.paragraphs.some((d) => d.id === id)
		).paragraphs

		onDeleteSubElement(paragraphs, id)

		setState({ ...state, documentation: state.documentation })
	}

	function onAddParagraph(event) {
		const element = state.documentation.find(
			(e) => e.id.toString() === event.currentTarget.id
		)

		const lastParagraph = sortDocElements(element.paragraphs)[
			element.paragraphs.length - 1
		]

		const paragraph = {
			id: getLargestParagraphId() + 1,
			previousId: lastParagraph?.id ?? 0,
			text: '',
			icon: null,
		}

		element.paragraphs.push(paragraph)

		setState({ ...state, documentation: state.documentation })
	}

	function onAddElement(event) {
		const sectionId = parseInt(event.currentTarget.id, 10)

		const filteredDocs = state.documentation.filter(
			(e) => e.sectionId === sectionId
		)

		const lastElement =
			sortDocElements(filteredDocs)[filteredDocs.length - 1]

		const element = {
			id: getLargestElementId() + 1,
			previousId: lastElement?.id ?? 0,
			sectionId: sectionId,
			title: {
				size: 5,
				text: '',
			},
			images: [],
			paragraphs: [
				{
					id: getLargestParagraphId() + 1,
					previousId: 0,
					text: '',
					icon: null,
				},
			],
			isEdit: true,
		}

		state.documentation.push(element)

		setState({ ...state, documentation: state.documentation })
	}

	async function onAddImages(event) {
		const files = event.target.files

		const element = state.documentation.find(
			(e) => e.id.toString() === event.currentTarget.id
		)

		const results = await postImages(files, element.id)
		let previousImage = sortDocElements(element.images)[
			element.images.length - 1
		]

		const largestImageId = getLargestImageId()

		for (let i = 0; i < results.length; i++) {
			const image = {
				id: largestImageId + i + 1,
				previousId: previousImage?.id ?? 0,
				fileId: results[i],
			}

			previousImage = image

			element.images.push(image)
		}

		setState({ ...state, documentation: state.documentation })
	}

	function getLargestParagraphId() {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)

		return Math.max(...paragraphs.map((e) => e.id), 0)
	}

	function getLargestElementId() {
		return Math.max(...state.documentation.map((e) => e.id), 0)
	}

	function getLargestImageId() {
		const images = state.documentation.map((e) => e.images).flat(1)

		return Math.max(...images.map((e) => e.id), 0)
	}

	function onDownImage(event) {
		const images = state.documentation.map((e) => e.images).flat(1)
		onDownSubElement(images, event.currentTarget.id)
		setState({ ...state, documentation: state.documentation })
	}

	function onUpImage(event) {
		const images = state.documentation.map((e) => e.images).flat(1)
		onUpSubElement(images, event.currentTarget.id)
		setState({ ...state, documentation: state.documentation })
	}

	function onDeleteImage(event) {
		const id = parseInt(event.currentTarget.id, 10)

		const images = state.documentation.find((e) =>
			e.images.some((d) => d.id === id)
		).images

		onDeleteSubElement(images, id)

		setState({ ...state, documentation: state.documentation })
	}

	console.log(state)

	if (state.loading) {
		return <></>
	}

	function renderDocSection(title, sectionId) {
		return (
			<>
				<Title text={title} size={2} />
				{sortDocElements(
					state.documentation.filter((e) => e.sectionId === sectionId)
				).map((e, i) => (
					<DynDocEditElement
						docElement={e}
						key={i}
						onUp={onUp}
						onDown={onDown}
						onUpParagraph={onUpParagraph}
						onDownParagraph={onDownParagraph}
						onEdit={onEdit}
						onEditOff={onEditOff}
						onTitleChange={onTitleChange}
						onParagraphChange={onParagraphChange}
						onTitleSizeChange={onTitleSizeChange}
						onParagraphIconChange={onParagraphIconChange}
						onAddParagraph={onAddParagraph}
						onElementDelete={onElementDelete}
						onParagraphDelete={onParagraphDelete}
						onAddImages={onAddImages}
						onDeleteImage={onDeleteImage}
						onUpImage={onUpImage}
						onDownImage={onDownImage}
						isLast={state.documentation.length - 1 === i}
					/>
				))}
				<Grid item xs={12}>
					<StyledButton
						onClick={onAddElement}
						id={sectionId}
						color="primary"
						variant={'contained'}
						fullWidth={true}
					>
						Neuer Abschnitt
					</StyledButton>
				</Grid>
			</>
		)
	}

	return (
		<NarrowLayout>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
				alignItems="flex-start"
				id={'checkinsAppTabName'}
			>
				<Title text="Anleitung Checkin KidsTown Edit" size={1} />
				<Grid item xs={12}>
					<p style={{ color: '#0366d6' }}>
						Hier kann die Anleitung bearbeitet werden. Jeder
						Anleitungsabschnitt kann einen Titel, mehrere
						Paragraphen und mehrere Bilder beinhalten.
					</p>
					<p style={{ color: '#0366d6' }}>
						Der Edditiermodus kann pro Abschnitt aktiviert und
						deaktiviert werden. Änderungen werden auch gespeichert,
						wenn der Edditiermodus deaktiviert ist.
					</p>
					<p style={{ color: '#0366d6' }}>
						Abschnitte, Paragraphen und Bilder können hinzugefügt,
						verschoben und gelöscht werden. Es können mehrere Bilder
						auf einmal hochgeladen werden, aber die Bilder, wie auch
						die Paragraphen, können nur innerhalb des Abschnitts
						verschoben werden. Abschnitte können nur innerhalb der
						Stationen verschoben werden. Ein neuer Abschnitt kann
						jeweils ganz am Ende der Station hinzugefügt werden.
					</p>
					<p style={{ color: '#0366d6' }}>
						Den Titel gibt es in drei verschieden Grössen und kann
						mit dem 'T's eingestellt werden. Wenn kein Titel
						gewünscht ist, Feld leer lassen. Bei den Paragraphen
						stehen drei verschiedene Symbole zur Verfügung. Leere
						Paragraphen werden nicht gespeichert.
					</p>
					<p style={{ color: '#0366d6' }}>
						<b>
							Alle Änderungen werden erst beim Speichern
							übernommen.
						</b>
					</p>
				</Grid>
				<Grid item xs={12}>
					<Button
						color={'success'}
						variant={'contained'}
						fullWidth={true}
						onClick={saveDoc}
						disabled={state.isSaving}
					>
						Anleitung speichern
					</Button>
				</Grid>
				{state.alert.text.length > 0 ? (
					<CheckInAlert alert={state.alert} />
				) : (
					<></>
				)}
				{renderDocSection('CheckIns App (Label Stationen)', 1)}
				{renderDocSection('Kidstown WebApp (Scan Stationen)', 2)}
			</Grid>
		</NarrowLayout>
	)
}

export const DynDocumentationEdit = withAuth(DynDocsEdit)
