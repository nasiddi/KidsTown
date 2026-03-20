import Grid from '@mui/material/Grid'
import React from 'react'

import type { DocElement, DocImage } from '../helpers/BackendClient'

import { sortDocElements } from './DynDocHelpers'
import { DynDocParagraph } from './DynDocParagraph'

interface DynDocElementProps {
  docElement: DocElement
}

export function DynDocElement({ docElement }: DynDocElementProps) {
  const element = docElement

  const [open, setOpen] = React.useState(6)

  function handleOpen() {
    if (open === 6) {
      setOpen(12)
    } else {
      setOpen(6)
    }
  }

  return (
    <Grid size={12} style={{ border: '3px', borderColor: 'black' }}>
      <Grid container spacing={1} justifyContent="space-between" alignItems="flex-start">
        {element.title ? (
          <Title text={element.title?.text} size={element.title?.size} gridItemSize={12} />
        ) : (
          <></>
        )}
        {stylizedText(element, element.images.length > 0 ? open : 12)}
        {element.images.length > 0 ? splitImageInner(element.images, handleOpen, open) : <></>}
      </Grid>
    </Grid>
  )
}

function stylizedText(element: DocElement, size: number) {
  const paragraphs =
    sortDocElements(element.paragraphs).map((p, index) => (
      <DynDocParagraph key={index} icon={p.icon} text={p.text} />
    )) || []

  const sm = size

  return <Grid size={{ sm: sm, xs: 12 }}>{paragraphs}</Grid>
}

interface DynDocImageProps {
  fileId: string
  onClick: () => void
}

export function DynDocImage({ fileId, onClick }: DynDocImageProps) {
  return (
    <Grid size={12} style={{ marginBottom: '10px' }}>
      <img
        src={`https://lh3.googleusercontent.com/d/${fileId}`}
        alt={fileId}
        style={{ maxWidth: '100%' }}
        onClick={onClick}
      />
    </Grid>
  )
}

function splitImageInner(images: DocImage[], func: () => void, open: number) {
  const mediaCards = sortDocElements(images).map((e) => (
    <DynDocImage key={e.id} fileId={e.fileId} onClick={func} />
  ))

  if (mediaCards.length === 0) {
    return <></>
  }

  const sm = open

  return (
    <Grid size={{ sm: sm, xs: 12 }}>
      <Grid container spacing={1} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>{mediaCards}</Grid>
      </Grid>
    </Grid>
  )
}

type HeadingTag = 'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'h6'

interface TitleProps {
  text: string
  size: number
  gridItemSize?: number
}

export function Title({ text, size, gridItemSize }: TitleProps) {
  const TitleSize = `h${size}` as HeadingTag

  return (
    <Grid size={gridItemSize}>
      <TitleSize>{text}</TitleSize>
    </Grid>
  )
}
