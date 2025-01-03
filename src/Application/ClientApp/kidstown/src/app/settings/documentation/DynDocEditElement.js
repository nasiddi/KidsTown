'use client'
import React from 'react'
import {Button, IconButton, TextField} from '@mui/material'
import Grid from '@mui/material/Grid2';
import EditIcon from '@mui/icons-material/Edit'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import TitleIcon from '@mui/icons-material/Title'
import DeleteIcon from '@mui/icons-material/Delete'
import EditOffIcon from '@mui/icons-material/EditOff'
import {DynDocEditParagraph} from './DynDocEditParagraph'
import {DynDocImage, stylizedText, Title} from '../../documentation/DynDocElement'
import {sortDocElements} from '../../documentation/DynDocHelpers'

export function DynDocEditElement(props) {
    const element = props.docElement

    function setTitleSizeStyle(size, selectedSize) {
        if (size === selectedSize) {
            return {
                backgroundColor: '#0366d6',
                color: 'white',
            }
        }

        return {
            border: '1px solid #0366d6',
        }
    }

    return (
        <Grid
            size={12}
            style={{
                border: '2px solid lightgrey',
                borderRadius: '5px',
                padding: '5px',
                marginBottom: '5px',
            }}
        >
            <Grid
                container
                spacing={0}
                justifyContent="space-between"
                alignItems="flex-start"
            >
                <Grid size={{sm: 0.5, xs: 12}}>
                    <Grid
                        container
                        direction="column"
                        justifyContent="flex-start"
                        alignItems="center"
                    >
                        <Grid>
                            <IconButton
                                color={'primary'}
                                onClick={props.onUp}
                                id={element.id.toString()}
                                disabled={element.previousId === 0}
                            >
                                <ExpandLessIcon/>
                            </IconButton>
                        </Grid>
                        <Grid>
                            {element.isEdit ? (
                                <IconButton
                                    color={'primary'}
                                    onClick={props.onEditOff}
                                    id={element.id.toString()}
                                >
                                    <EditOffIcon/>
                                </IconButton>
                            ) : (
                                <IconButton
                                    color={'primary'}
                                    onClick={props.onEdit}
                                    id={element.id.toString()}
                                >
                                    <EditIcon/>
                                </IconButton>
                            )}
                            <Grid>
                                <IconButton
                                    color={'error'}
                                    onClick={props.onElementDelete}
                                    id={element.id.toString()}
                                >
                                    <DeleteIcon/>
                                </IconButton>
                            </Grid>
                        </Grid>
                        <Grid>
                            <IconButton
                                color={'primary'}
                                onClick={props.onDown}
                                id={element.id.toString()}
                                disabled={props.isLast}
                            >
                                <ExpandMoreIcon/>
                            </IconButton>
                        </Grid>
                    </Grid>
                </Grid>
                <Grid size={11.5}>
                    <Grid
                        container
                        spacing={1}
                        justifyContent="space-between"
                        alignItems="flex-start"
                    >
                        {element.title ? (
                            <Title
                                text={element.title?.text}
                                size={element.title?.size}
                                gridItemSize={props?.onEdit ? 11 : 12}
                            />
                        ) : (
                            <></>
                        )}
                        {stylizedText(
                            element,
                            element.images?.length > 0 ? 6 : 11
                        )}
                        {element.images?.length > 0 ? (
                            splitImageInner(props)
                        ) : (
                            <></>
                        )}
                        {element.isEdit ? (
                            <Grid size={12}>
                                <Grid
                                    container
                                    spacing={1}
                                    direction="row"
                                    justifyContent="space-between"
                                    alignItems="center"
                                >
                                    <Grid size={{sm: 6, xs: 12}}>
                                        <Button
                                            id={element.id}
                                            onClick={props.onAddParagraph}
                                            color="primary"
                                            variant={'contained'}
                                            fullWidth={true}
                                        >
                                            Neuer Paragraph
                                        </Button>
                                    </Grid>
                                    <Grid size={{sm: 6, xs: 12}}>
                                        <Button
                                            variant="contained"
                                            component="label"
                                            fullWidth={true}
                                            disabled={props.isSaving}
                                        >
                                            Bilder hinzufügen
                                            <input
                                                id={element.id.toString()}
                                                hidden
                                                accept="image/*"
                                                multiple
                                                type="file"
                                                onChange={props.onAddImages}
                                            />
                                        </Button>
                                    </Grid>
                                    <Grid size={{sm: 10, xs: 12}}>
                                        <TextField
                                            id={element.id.toString()}
                                            label={'Title'}
                                            fullWidth={true}
                                            value={element.title.text}
                                            onChange={props.onTitleChange}
                                            style={{marginTop: '5px'}}
                                        />
                                    </Grid>
                                    <Grid>
                                        <IconButton
                                            size={'small'}
                                            color={'primary'}
                                            onClick={props.onTitleSizeChange}
                                            id={element.id}
                                            name={'5'}
                                            style={setTitleSizeStyle(
                                                5,
                                                element.title.size
                                            )}
                                        >
                                            <TitleIcon/>
                                        </IconButton>
                                    </Grid>
                                    <Grid>
                                        <IconButton
                                            size={'medium'}
                                            color={'primary'}
                                            onClick={props.onTitleSizeChange}
                                            id={element.id}
                                            name={'4'}
                                            style={setTitleSizeStyle(
                                                4,
                                                element.title.size
                                            )}
                                        >
                                            <TitleIcon/>
                                        </IconButton>
                                    </Grid>
                                    <Grid>
                                        <IconButton
                                            size={'large'}
                                            color={'primary'}
                                            onClick={props.onTitleSizeChange}
                                            id={element.id}
                                            name={'3'}
                                            style={setTitleSizeStyle(
                                                3,
                                                element.title.size
                                            )}
                                        >
                                            <TitleIcon/>
                                        </IconButton>
                                    </Grid>
                                    {sortDocElements(element.paragraphs).map(
                                        (e, i) => (
                                            <DynDocEditParagraph
                                                key={e.id}
                                                paragraph={e}
                                                onUpParagraph={
                                                    props.onUpParagraph
                                                }
                                                onDownParagraph={
                                                    props.onDownParagraph
                                                }
                                                onParagraphIconChange={
                                                    props.onParagraphIconChange
                                                }
                                                onParagraphChange={
                                                    props.onParagraphChange
                                                }
                                                onParagraphDelete={
                                                    props.onParagraphDelete
                                                }
                                                isLast={
                                                    element.paragraphs.length -
                                                    1 ===
                                                    i
                                                }
                                            />
                                        )
                                    )}
                                </Grid>
                            </Grid>
                        ) : (
                            <></>
                        )}
                    </Grid>
                </Grid>
            </Grid>
        </Grid>
    )
}

function DynDocEditImage(props) {
    const img = props.image

    return (
        <Grid
            size={12}
            style={{
                border: '2px solid lightgrey',
                borderRadius: '5px',
                padding: '5px',
                marginBottom: '5px',
            }}
        >
            <Grid
                container
                spacing={1}
                justifyContent="space-between"
                alignItems="flex-start"
            >
                <Grid size={12}>
                    <img
                        src={`https://lh3.googleusercontent.com/d/${img.fileId}`}
                        alt={img.fileId}
                        style={{maxWidth: "100%"}}
                    />
                </Grid>
                <Grid>
                    <IconButton
                        color={'primary'}
                        onClick={props.onUp}
                        id={img.id.toString()}
                        disabled={img.previousId === 0}
                    >
                        <ExpandLessIcon/>
                    </IconButton>
                </Grid>
                <Grid>
                    <IconButton
                        color={'error'}
                        onClick={props.onDelete}
                        id={img.id.toString()}
                    >
                        <DeleteIcon/>
                    </IconButton>
                </Grid>
                <Grid>
                    <IconButton
                        color={'primary'}
                        onClick={props.onDown}
                        id={img.id.toString()}
                        disabled={props.isLast}
                    >
                        <ExpandMoreIcon/>
                    </IconButton>
                </Grid>
            </Grid>
        </Grid>
    )
}

function splitImageInner(props) {
    const mediaCards = sortDocElements(props.docElement.images).map((e, i) =>
        props.docElement.isEdit ? (
            <DynDocEditImage
                key={e.id}
                image={e}
                onUp={props.onUpImage}
                onDown={props.onDownImage}
                onDelete={props.onDeleteImage}
                isLast={props.docElement.images.length - 1 === i}
            />
        ) : (
            <DynDocImage key={e.id} fileId={e.fileId}/>
        )
    )

    return (
        <Grid size={{sm: 6, xs: 12}}>
            <Grid
                container
                spacing={1}
                justifyContent="space-between"
                alignItems="flex-start"
            >
                <Grid size={12}>
                    {mediaCards}
                </Grid>
            </Grid>
        </Grid>
    )
}
