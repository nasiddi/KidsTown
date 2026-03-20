import Grid from '@mui/material/Grid'

import { NarrowLayout, WideLayout } from '../components/Layout'
import withAuth from '../components/withAuth'

import OverviewDetail from './OverviewDetail'
import OverviewFilter from './OverviewFilter'
import OverviewHeadCount from './OverViewHeadCounts'
import OverviewOptions from './OverviewOptions'

function Over() {
  return (
    <>
      <NarrowLayout>
        <Grid container spacing={3} justifyContent="space-between" alignItems="flex-start">
          <Grid size={12}>
            <OverviewOptions />
          </Grid>
          <Grid size={12}>
            <OverviewHeadCount />
          </Grid>
          <Grid size={12}>
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

const OverviewPage = withAuth(Over)
export default OverviewPage
