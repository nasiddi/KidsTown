'use client'
import React from 'react'
import OverviewOptions from './OverviewOptions'
import OverviewHeadCount from './OverViewHeadCounts'
import OverviewFilter from './OverviewFilter'
import Grid from '@mui/material/Grid2';
import OverviewDetail from './OverviewDetail'
import {NarrowLayout, WideLayout} from "@/app/components/Layout";
import withAuth from "@/app/components/withAuth";

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
                    <Grid size={12}>
                        <OverviewOptions/>
                    </Grid>
                    <Grid size={12}>
                        <OverviewHeadCount/>
                    </Grid>
                    <Grid size={12}>
                        <OverviewFilter/>
                    </Grid>
                </Grid>
            </NarrowLayout>
            <br/>
            <WideLayout>
                <OverviewDetail/>
            </WideLayout>
        </>
    )
}

export default withAuth(Over);

