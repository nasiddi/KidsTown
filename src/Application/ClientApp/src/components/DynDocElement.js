import React from 'react'
import { Grid } from '@mui/material'
import { Title } from './DocElements'
import Image from 'mui-image'
import { DynDocParagraph } from './DynDocParagraph'

export function DynDocElement(props) {
	const element = props.docElement

	const [open, setOpen] = React.useState(6)

	function handleOpen() {
		if (open === 6) {
			setOpen(12)
		} else {
			setOpen(6)
		}
	}

	return (
		<Grid item xs={12} style={{ border: '3px', borderColor: 'black' }}>
			<Grid
				container
				spacing={1}
				justifyContent="space-between"
				alignItems="flex-start"
			>
				{element.title ? (
					<Title
						text={element.title?.text}
						size={element.title?.size}
						gridItemSize={12}
					/>
				) : (
					<></>
				)}
				{stylizedText(element, element.imageUrl ? open : 12)}
				{element.imageUrl ? (
					splitImageInner(element.imageUrl, handleOpen, open)
				) : (
					<></>
				)}
			</Grid>
		</Grid>
	)
}

export function sortParagraphs(paragraphs) {
	const sorted = []

	let nextParagraph = paragraphs.find((e) => e.previousParagraphId === 0)
	let previousId = 0

	while (nextParagraph) {
		sorted.push(nextParagraph)
		previousId = nextParagraph.paragraphId
		nextParagraph = findNextParagraph(previousId, paragraphs)
	}

	return sorted
}

function findNextParagraph(previousId, paragraphs) {
	return paragraphs.find((e) => e.previousParagraphId === previousId)
}

export function stylizedText(element, size) {
	const paragraphs =
		sortParagraphs(element.paragraphs || []).map((p, index) => (
			<DynDocParagraph key={index} icon={p.icon} text={p['text']} />
		)) || []

	return (
		<Grid item sm={size} xs={12}>
			{paragraphs}
		</Grid>
	)
}

export function splitImageInner(fileName, func, open) {
	let image = ''
	try {
		image = require(`../docs/${fileName}`)
	} catch (e) {}

	const mediaCard = <Image src={image} alt={fileName} onClick={func} fluid />

	return (
		<Grid item sm={open} xs={12}>
			{mediaCard}
		</Grid>
	)
}
