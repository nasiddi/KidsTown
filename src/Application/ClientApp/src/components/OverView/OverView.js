import React from 'react'
import OverviewOptions from './OverviewOptions'
import OverviewHeadCount from './OverViewHeadCounts'
import OverviewFilter from './OverviewFilter'
import { withAuth } from '../../auth/MsalAuthProvider'
import { Grid } from '@mui/material'
import { NarrowLayout, WideLayout } from '../Layout'
import OverviewDetail from './OverviewDetail'

function Over() {
	return (
		<>
			<NarrowLayout>
				<Grid
					container
					spacing={3}
					justifyContent="space-between"
					alignItems="flex-start"
				>
					<Grid item xs={12}>
						<OverviewOptions />
					</Grid>
					<Grid item xs={12}>
						<OverviewHeadCount />
					</Grid>
					<Grid item xs={12}>
						<OverviewFilter />
					</Grid>
				</Grid>
			</NarrowLayout>
			<br />
			<WideLayout>
				<OverviewDetail />
			</WideLayout>
		</>
	)
}

export const OverView = withAuth(Over)
