import React from 'react'
import Grid from '@mui/material/Grid'
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

export function stylizedText(element, size) {
  const paragraphs =
    sortDocElements(element.paragraphs).map((p, index) => (
      <DynDocParagraph key={index} icon={p.icon} text={p['text']} />
    )) || []

  const sm = parseInt(size, 10)

  return <Grid size={{ sm: sm, xs: 12 }}>{paragraphs}</Grid>
}

export function DynDocImage(props) {
  return (
    <Grid size={12} style={{ marginBottom: '10px' }}>
      <img
        src={`https://lh3.googleusercontent.com/d/${props.fileId}`}
        alt={props.fileId}
        style={{ maxWidth: '100%' }}
        onClick={props.onClick}
      />
    </Grid>
  )
}

function splitImageInner(images, func, open) {
  const mediaCards = sortDocElements(images).map((e) => (
    <DynDocImage key={e.id} fileId={e.fileId} onClick={func} />
  ))

  if (mediaCards.length === 0) {
    return <></>
  }

  const sm = parseInt(open, 10)

  return (
    <Grid size={{ sm: sm, xs: 12 }}>
      <Grid container spacing={1} justifyContent="space-between" alignItems="flex-start">
        <Grid size={12}>{mediaCards}</Grid>
      </Grid>
    </Grid>
  )
}

export function Title(props) {
  const TitleSize = `h${props['size']}`

  return (
    <Grid size={props.gridItemSize}>
      <TitleSize>{props['text']}</TitleSize>
    </Grid>
  )
}
