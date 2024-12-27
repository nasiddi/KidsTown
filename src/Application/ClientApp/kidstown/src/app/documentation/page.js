'use client'
import React, {useEffect, useState} from 'react'
import Grid from '@mui/material/Grid2';
import {sortDocElements} from "./DynDocHelpers";
import {DynDocElement, Title} from "./DynDocElement";
import {NarrowLayout} from "@/app/components/Layout";
import {fetchDocumentation} from "@/app/helpers/BackendClient";

export default function Documentation() {
    return (
        <NarrowLayout>
            <DynDocs/>
        </NarrowLayout>
    )
}

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
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    console.log(state)

    if (state.loading) {
        return <></>
    }

    function renderDocSection(title, sectionId) {
        return (
            <>
                <Title text={title} size={2}/>
                {sortDocElements(
                    state.documentation.filter((e) => e.sectionId === sectionId)
                ).map((e, i) => (
                    <DynDocElement docElement={e} key={i}/>
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
