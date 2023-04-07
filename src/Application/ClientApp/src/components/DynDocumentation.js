import React from 'react'
import { Grid } from '@mui/material'
import { NarrowLayout } from './Layout'
import { useEffect, useState } from 'react'
import { Title } from './DocElements'
import { DynDocElement } from './DynDocElement'

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

	async function fetchDocumentation() {
		const response = await fetch('documentation')

		return await response.json()
	}

	console.log(state)

	if (state.loading) {
		return <></>
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
				<Title text="Anleitung Checkin KidsTown" size={1} />
				<Title
					text="CheckIns App (Label Stationen)"
					size={2}
					id={'CheckInsApp'}
				/>
				{state.documentation.map((e, i) => (
					<DynDocElement docElement={e} key={i} />
				))}
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
