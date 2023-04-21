import React from 'react'
import { Grid } from '@mui/material'
import { NarrowLayout } from '../Layout'
import { useEffect, useState } from 'react'
import { DynDocElement, Title } from './DynDocElement'
import { fetchDocumentation, sortDocElements } from './DynDocHelpers'

function DynDocs() {
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
					<DynDocElement docElement={e} key={i} />
				))}
			</>
		)
	}

	return (
		<div>
			<Grid
				container
				spacing={3}
				justifyContent="space-between"
				alignItems="flex-start"
				id={'checkinsAppTabName'}
			>
				<Title
					text="Anleitung Checkin KidsTown"
					size={1}
					gridItemSize={12}
				/>
				{renderDocSection('CheckIns App (Label Stationen)', 1)}
				{renderDocSection('Kidstown WebApp (Scan Stationen)', 2)}
			</Grid>
		</div>
	)
}

export default function DynDocumentation() {
	return (
		<NarrowLayout>
			<DynDocs />
		</NarrowLayout>
	)
}
