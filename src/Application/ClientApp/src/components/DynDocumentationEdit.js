import React from 'react'
import { Grid } from '@mui/material'
import { useEffect, useState } from 'react'
import { DynDocEditElement } from './DynDocEditElement'
import { Title } from './DocElements'
import { withAuth } from '../auth/MsalAuthProvider'
import { NarrowLayout } from './Layout'
import { StyledButton } from './Common'
import { sortParagraphs } from './DynDocElement'

function DynDocsEdit() {
	const [state, setState] = useState({
		documentation: [],
		loading: true,
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

	async function fetchDocumentation() {
		const response = await fetch('documentation')

		return await response.json()
	}

	function onEdit(event) {
		const element = state.documentation.find(
			(e) => e.elementId.toString() === event.currentTarget.id
		)

		element.isEdit = true

		setState({ ...state, documentation: state.documentation })
	}

	function onEditOff(event) {
		const element = state.documentation.find(
			(e) => e.elementId.toString() === event.currentTarget.id
		)

		element.isEdit = false

		setState({ ...state, documentation: state.documentation })
	}

	function onUp(event) {
		const element = state.documentation.find(
			(e) => e.elementId.toString() === event.currentTarget.id
		)

		const previous = state.documentation.find(
			(e) => e.elementId === element.previousElementId
		)

		const following = state.documentation.find(
			(e) => e.previousElementId === element.elementId
		)

		const upperPreviousElementId = previous.previousElementId

		previous.previousElementId = element.elementId
		element.previousElementId = upperPreviousElementId

		if (following) {
			following.previousElementId = previous.elementId
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onUpParagraph(event) {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)

		const paragraph = paragraphs.find(
			(e) => e.paragraphId.toString() === event.currentTarget.id
		)

		const previous = paragraphs.find(
			(e) => e.paragraphId === paragraph.previousParagraphId
		)

		const following = paragraphs.find(
			(e) => e.previousParagraphId === paragraph.paragraphId
		)

		const upperPreviousParagraphId = previous.previousParagraphId

		previous.previousParagraphId = paragraph.paragraphId
		paragraph.previousParagraphId = upperPreviousParagraphId

		if (following) {
			following.previousParagraphId = previous.paragraphId
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onDown(event) {
		const element = state.documentation.find(
			(e) => e.elementId.toString() === event.currentTarget.id
		)

		const previous = state.documentation.find(
			(e) => e.elementId === element.previousElementId
		)

		const following = state.documentation.find(
			(e) => e.previousElementId === element.elementId
		)

		const followingFollowing = state.documentation.find(
			(e) => e.previousElementId === following.elementId
		)

		following.previousElementId = previous?.elementId || 0
		element.previousElementId = following.elementId

		if (followingFollowing) {
			followingFollowing.previousElementId = element.elementId
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onElementDelete(event) {
		const elementId = parseInt(event.currentTarget.id, 10)

		const element = state.documentation.find(
			(e) => e.elementId === elementId
		)

		const previous = state.documentation.find(
			(e) => e.elementId === element.previousElementId
		)

		const following = state.documentation.find(
			(e) => e.previousElementId === element.elementId
		)

		const index = state.documentation.findIndex(
			(e) => e.elementId === element.elementId
		)

		state.documentation.splice(index, 1)

		if (following) {
			following.previousElementId = previous?.elementId || 0
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onDownParagraph(event) {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)

		const paragraph = paragraphs.find(
			(e) => e.paragraphId?.toString() === event.currentTarget.id
		)

		const previous = paragraphs.find(
			(e) => e.paragraphId === paragraph.previousParagraphId
		)

		const following = paragraphs.find(
			(e) => e.previousParagraphId === paragraph.paragraphId
		)

		const followingFollowing = paragraphs.find(
			(e) => e.previousParagraphId === following.paragraphId
		)

		following.previousParagraphId = previous?.paragraphId || 0
		paragraph.previousParagraphId = following.paragraphId

		if (followingFollowing) {
			followingFollowing.previousParagraphId = paragraph.paragraphId
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onTitleChange(event) {
		const element = state.documentation.find(
			(e) => e.elementId.toString() === event.currentTarget.id
		)

		element.title.text = event.currentTarget.value

		setState({ ...state, documentation: state.documentation })
	}

	function onParagraphChange(event) {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)

		const paragraph = paragraphs.find(
			(e) => e.paragraphId?.toString() === event.currentTarget.id
		)

		paragraph.text = event.currentTarget.value

		setState({ ...state, documentation: state.documentation })
	}

	function onTitleSizeChange(event) {
		const element = state.documentation.find(
			(e) => e.elementId.toString() === event.currentTarget.id
		)

		element.title.size = parseInt(event.currentTarget.name, 10)

		setState({ ...state, documentation: state.documentation })
	}

	function onParagraphIconChange(event) {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)

		const paragraph = paragraphs.find(
			(e) => e.paragraphId?.toString() === event.currentTarget.id
		)

		if (paragraph.icon === event.currentTarget.name) {
			paragraph.icon = null
		} else {
			paragraph.icon = event.currentTarget.name
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onParagraphDelete(event) {
		const paragraphId = parseInt(event.currentTarget.id, 10)

		const paragraphs = state.documentation.find((e) =>
			e.paragraphs.some((d) => d.paragraphId === paragraphId)
		).paragraphs

		const paragraph = paragraphs.find((e) => e.paragraphId === paragraphId)

		const previous = paragraphs.find(
			(e) => e.paragraphId === paragraph.previousParagraphId
		)

		const following = paragraphs.find(
			(e) => e.previousParagraphId === paragraph.paragraphId
		)

		const index = paragraphs.findIndex(
			(e) => e.paragraphId === paragraph.paragraphId
		)
		paragraphs.splice(index, 1)

		if (following) {
			following.previousParagraphId = previous?.paragraphId || 0
		}

		setState({ ...state, documentation: state.documentation })
	}

	function onAddParagraph(event) {
		const element = state.documentation.find(
			(e) => e.elementId.toString() === event.currentTarget.id
		)

		const lastParagraph = sortParagraphs(element.paragraphs)[
			element.paragraphs.length - 1
		]

		const paragraph = {
			paragraphId: getLargestParagraphId() + 1,
			previousParagraphId: lastParagraph?.paragraphId ?? 0,
			text: '',
			icon: null,
		}

		element.paragraphs.push(paragraph)

		setState({ ...state, documentation: state.documentation })
	}

	function onAddElement() {
		const lastElement = sortDocs(state.documentation)[
			state.documentation.length - 1
		]

		const element = {
			elementId: getLargestElementId() + 1,
			previousElementId: lastElement?.elementId ?? 0,
			title: {
				size: 5,
				text: '',
			},
			paragraphs: [
				{
					paragraphId: getLargestParagraphId() + 1,
					previousParagraphId: 0,
					text: '',
					icon: null,
				},
			],
			isEdit: true,
		}

		state.documentation.push(element)

		setState({ ...state, documentation: state.documentation })
	}

	function getLargestParagraphId() {
		const paragraphs = state.documentation.map((e) => e.paragraphs).flat(1)

		return Math.max(...paragraphs.map((e) => e.paragraphId))
	}

	function getLargestElementId() {
		return Math.max(...state.documentation.map((e) => e.elementId))
	}

	console.log(state)

	if (state.loading) {
		return <></>
	}

	function sortDocs(docs) {
		const sorted = []

		let nextElement = docs.find((e) => e.previousElementId === 0)
		let previousId = 0

		while (nextElement) {
			sorted.push(nextElement)
			previousId = nextElement.elementId
			nextElement = findNextElement(previousId, docs)
		}

		return sorted
	}

	function findNextElement(previousId, docs) {
		return docs.find((e) => e.previousElementId === previousId)
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
						Hier kann die Anleitung bearbeitet werden.
					</p>
				</Grid>
				<Title
					text="CheckIns App (Label Stationen)"
					size={2}
					id={'CheckInsApp'}
				/>
				{sortDocs(state.documentation).map((e, i) => (
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
						isLast={state.documentation.length - 1 === i}
					/>
				))}
				<Grid item xs={12}>
					<StyledButton
						onClick={onAddElement}
						color="primary"
						variant={'contained'}
						fullWidth={true}
					>
						Neuer Abschnitt
					</StyledButton>
				</Grid>
			</Grid>
		</NarrowLayout>
	)
}

export const DynDocumentationEdit = withAuth(DynDocsEdit)
