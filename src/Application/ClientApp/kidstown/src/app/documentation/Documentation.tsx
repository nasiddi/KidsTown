import { Alert, Box, CircularProgress } from '@mui/material'
import Grid from '@mui/material/Grid'
import { useEffect, useState } from 'react'

import { NarrowLayout } from '../components/Layout'
import type { DocElement } from '../helpers/BackendClient'
import { fetchDocumentation } from '../helpers/BackendClient'

import { DynDocElement, Title } from './DynDocElement'
import { sortDocElements } from './DynDocHelpers'

export default function Documentation() {
  return (
    <NarrowLayout>
      <DynDocs />
    </NarrowLayout>
  )
}

interface DynDocsState {
  documentation: DocElement[]
  loading: boolean
  error: string | null
}

function DynDocs() {
  const [state, setState] = useState<DynDocsState>({
    documentation: [],
    loading: true,
    error: null,
  })

  useEffect(() => {
    async function load() {
      try {
        const documentation = await fetchDocumentation()
        setState((prev) => ({
          ...prev,
          documentation,
          loading: false,
        }))
      } catch {
        setState((prev) => ({ ...prev, loading: false, error: 'Failed to load.' }))
      }
    }

    load().then()
  }, [])

  if (state.loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (state.error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        {state.error}
      </Alert>
    )
  }

  function renderDocSection(title: string, sectionId: number) {
    return (
      <>
        <Title text={title} size={2} />
        {sortDocElements(state.documentation.filter((e) => e.sectionId === sectionId)).map(
          (e, i) => (
            <DynDocElement docElement={e} key={i} />
          ),
        )}
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
        <Title text="Anleitung Checkin KidsTown" size={1} gridItemSize={12} />
        {renderDocSection('CheckIns App (Label Stationen)', 1)}
        {renderDocSection('Kidstown WebApp (Scan Stationen)', 2)}
      </Grid>
    </div>
  )
}
