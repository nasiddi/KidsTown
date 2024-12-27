'use client'
import React from 'react'
import {IconButton, TextField} from '@mui/material'
import Grid from '@mui/material/Grid2';
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import ArrowCircleRightOutlinedIcon from '@mui/icons-material/ArrowCircleRightOutlined'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'
import DeleteIcon from '@mui/icons-material/Delete'

export function DynDocEditParagraph(props) {
    const paragraph = props.paragraph

    return (
        <Grid size={12} key={paragraph.id}>
            <Grid
                container
                spacing={1}
                direction="row"
                justifyContent="space-between"
                alignItems="center"
            >
                <Grid>
                    <Grid
                        container
                        spacing={0}
                        direction="column"
                        justifyContent="space-around"
                        alignItems="center"
                    >
                        <Grid>
                            <IconButton
                                color={'primary'}
                                onClick={props.onUpParagraph}
                                id={paragraph.id.toString()}
                                disabled={paragraph.previousId === 0}
                            >
                                <ExpandLessIcon/>
                            </IconButton>
                        </Grid>
                        <Grid>
                            <IconButton
                                color={'primary'}
                                onClick={props.onDownParagraph}
                                id={paragraph.id.toString()}
                                disabled={props.isLast}
                            >
                                <ExpandMoreIcon/>
                            </IconButton>
                        </Grid>
                    </Grid>
                </Grid>
                <Grid>
                    <IconButton
                        size={'medium'}
                        color={paragraph.icon === 'Action' ? 'primary' : ''}
                        onClick={props.onParagraphIconChange}
                        id={paragraph.id}
                        name={'Action'}
                    >
                        <ArrowCircleRightOutlinedIcon/>
                    </IconButton>
                </Grid>
                <Grid>
                    <IconButton
                        size={'medium'}
                        color={paragraph.icon === 'Info' ? 'primary' : ''}
                        onClick={props.onParagraphIconChange}
                        id={paragraph.id}
                        name={'Info'}
                    >
                        <InfoOutlinedIcon/>
                    </IconButton>
                </Grid>
                <Grid>
                    <IconButton
                        size={'medium'}
                        color={paragraph.icon === 'Warning' ? 'primary' : ''}
                        onClick={props.onParagraphIconChange}
                        id={paragraph.id}
                        name={'Warning'}
                    >
                        <WarningAmberOutlinedIcon/>
                    </IconButton>
                </Grid>
                <Grid size={9}>
                    <TextField
                        value={paragraph.text}
                        id={paragraph.id.toString()}
                        onChange={props.onParagraphChange}
                        multiline
                        fullWidth={true}
                        inputProps={{maxLength: 1000}}
                    />
                </Grid>
                <Grid>
                    <IconButton
                        color={'error'}
                        onClick={props.onParagraphDelete}
                        id={paragraph.id.toString()}
                    >
                        <DeleteIcon/>
                    </IconButton>
                </Grid>
            </Grid>
        </Grid>
    )
}
