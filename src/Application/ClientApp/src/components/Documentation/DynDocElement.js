import React from 'react'
import { Grid } from '@mui/material'
import Image from 'mui-image'
import { DynDocParagraph } from './DynDocParagraph'
import { sortDocElements } from './DynDocHelpers'

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
				{stylizedText(element, element.images.length > 0 ? open : 12)}
				{element.images.length > 0 ? (
					splitImageInner(element.images, handleOpen, open)
				) : (
					<></>
				)}
			</Grid>
		</Grid>
	)
}

export function stylizedText(element, size) {
	const paragraphs =
		sortDocElements(element.paragraphs).map((p, index) => (
			<DynDocParagraph key={index} icon={p.icon} text={p['text']} />
		)) || []

	return (
		<Grid item sm={size} xs={12}>
			{paragraphs}
		</Grid>
	)
}

export function DynDocImage(props) {
	return (
		<Grid item xs={12} style={{ marginBottom: '10px' }}>
			<Image
				src={`https://lh3.googleusercontent.com/d/${props.fileId}`}
				alt={props.fileId}
				onClick={props.onClick}
				fluid
			/>
		</Grid>
	)
}

function splitImageInner(images, func, open) {
	const mediaCards = sortDocElements(images).map((e) => (
		<DynDocImage key={e.id} fileId={e.fileId} onClick={func} />
	))

	return (
		<Grid item sm={open} xs={12}>
			<Grid
				container
				spacing={1}
				justifyContent="space-between"
				alignItems="flex-start"
			>
				<Grid item xs={12}>
					{mediaCards}
				</Grid>
			</Grid>
		</Grid>
	)
}

export function Title(props) {
	const TitleSize = `h${props['size']}`

	return (
		<Grid item xs={props.gridItemSize}>
			<TitleSize>{props['text']}</TitleSize>
		</Grid>
	)
}
