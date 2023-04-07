import React from 'react'
import { Grid } from '@mui/material'
import Image from 'mui-image'
import ArrowCircleRightOutlinedIcon from '@mui/icons-material/ArrowCircleRightOutlined'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'

const action = 'action'
const info = 'info'
const warning = 'warning'

export function DocsIcon(props) {
	if (props.name === action) {
		return <ArrowCircleRightOutlinedIcon />
	}

	if (props.name === info) {
		return <InfoOutlinedIcon />
	}

	return <WarningAmberOutlinedIcon />
}

export function Title(props) {
	const TitleSize = `h${props['size']}`

	return (
		<Grid item xs={props.gridItemSize}>
			<TitleSize>{props['text']}</TitleSize>
			<a className="anchor hash" id={props['id']} href={props['id']} />
		</Grid>
	)
}

function splitImageInner(props, func, open) {
	let image = ''
	try {
		image = require(`../docs/${props['fileName']}`)
	} catch (e) {}

	const mediaCard = (
		<Image src={image} alt={props['fileName']} onClick={func} fluid />
	)

	return (
		<Grid item sm={open} xs={12}>
			{mediaCard}
		</Grid>
	)
}

// todo delete this once old doc is removed
export function FullText(props) {
	return stylizedText(props, 12)
}

export function TextImageSplit(props) {
	const [open, setOpen] = React.useState(6)
	function handleOpen() {
		if (open === 6) {
			setOpen(12)
		} else {
			setOpen(6)
		}
	}

	return (
		<Grid item xs={12}>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
				alignItems="flex-start"
			>
				{stylizedText(props, open)}
				{splitImageInner(props, handleOpen, open)}
			</Grid>
		</Grid>
	)
}
