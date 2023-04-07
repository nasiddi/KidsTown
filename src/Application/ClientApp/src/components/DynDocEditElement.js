import React from 'react'
import { Button, Grid, IconButton, TextField } from '@mui/material'
import EditIcon from '@mui/icons-material/Edit'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import TitleIcon from '@mui/icons-material/Title'
import DeleteIcon from '@mui/icons-material/Delete'
import EditOffIcon from '@mui/icons-material/EditOff'
import { DynDocEditParagraph } from './DynDocEditParagraph'
import { sortParagraphs, splitImageInner, stylizedText } from './DynDocElement'
import { Title } from './DocElements'

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

	function onImageClick() {}

	return (
		<Grid
			item
			xs={12}
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
				<Grid item sm={0.5} xs={12}>
					<Grid
						container
						direction="column"
						justifyContent="flex-start"
						alignItems="center"
					>
						<Grid item>
							<IconButton
								color={'primary'}
								onClick={props.onUp}
								id={element.elementId.toString()}
								disabled={element.previousElementId === 0}
							>
								<ExpandLessIcon />
							</IconButton>
						</Grid>
						<Grid item>
							{element.isEdit ? (
								<IconButton
									color={'primary'}
									onClick={props.onEditOff}
									id={element.elementId.toString()}
								>
									<EditOffIcon />
								</IconButton>
							) : (
								<IconButton
									color={'primary'}
									onClick={props.onEdit}
									id={element.elementId.toString()}
								>
									<EditIcon />
								</IconButton>
							)}
							<Grid item>
								<IconButton
									color={'error'}
									onClick={props.onElementDelete}
									id={element.elementId.toString()}
								>
									<DeleteIcon />
								</IconButton>
							</Grid>
						</Grid>
						<Grid item>
							<IconButton
								color={'primary'}
								onClick={props.onDown}
								id={element.elementId.toString()}
								disabled={props.isLast}
							>
								<ExpandMoreIcon />
							</IconButton>
						</Grid>
					</Grid>
				</Grid>
				<Grid item xs={11.5}>
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
								gridItemSize={props.onEdit ? 11 : 12}
							/>
						) : (
							<></>
						)}
						{stylizedText(element, element.imageUrl ? 6 : 11)}
						{element.imageUrl ? (
							splitImageInner(element.imageUrl, onImageClick, 5)
						) : (
							<></>
						)}
						{element.isEdit ? (
							<Grid item xs={12}>
								<Grid
									container
									spacing={1}
									direction="row"
									justifyContent="space-between"
									alignItems="center"
								>
									<Grid item sm={10} xs={12}>
										<TextField
											id={element.elementId.toString()}
											label={'Titel'}
											fullWidth={true}
											value={element.title.text}
											onChange={props.onTitleChange}
										/>
									</Grid>
									<Grid item>
										<IconButton
											size={'small'}
											color={'primary'}
											onClick={props.onTitleSizeChange}
											id={element.elementId}
											name={'5'}
											style={setTitleSizeStyle(
												5,
												element.title.size
											)}
										>
											<TitleIcon />
										</IconButton>
									</Grid>
									<Grid item>
										<IconButton
											size={'medium'}
											color={'primary'}
											onClick={props.onTitleSizeChange}
											id={element.elementId}
											name={'4'}
											style={setTitleSizeStyle(
												4,
												element.title.size
											)}
										>
											<TitleIcon />
										</IconButton>
									</Grid>
									<Grid item>
										<IconButton
											size={'large'}
											color={'primary'}
											onClick={props.onTitleSizeChange}
											id={element.elementId}
											name={'3'}
											style={setTitleSizeStyle(
												3,
												element.title.size
											)}
										>
											<TitleIcon />
										</IconButton>
									</Grid>
									{sortParagraphs(element.paragraphs).map(
										(e, i) => (
											<DynDocEditParagraph
												key={e.paragraphId}
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
									<Grid item xs={12}>
										<Button
											id={element.elementId}
											onClick={props.onAddParagraph}
											color="primary"
											variant={'outlined'}
											fullWidth={true}
										>
											Neuer Paragraph
										</Button>
									</Grid>
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
