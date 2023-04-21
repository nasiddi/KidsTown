import React from 'react'
import { Grid, IconButton, TextField } from '@mui/material'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import ArrowCircleRightOutlinedIcon from '@mui/icons-material/ArrowCircleRightOutlined'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'
import DeleteIcon from '@mui/icons-material/Delete'

export function DynDocEditParagraph(props) {
	const paragraph = props.paragraph

	return (
		<Grid item xs={12} key={paragraph.id}>
			<Grid
				container
				spacing={1}
				direction="row"
				justifyContent="space-between"
				alignItems="center"
			>
				<Grid item>
					<Grid
						container
						spacing={0}
						direction="column"
						justifyContent="space-around"
						alignItems="center"
					>
						<Grid item>
							<IconButton
								color={'primary'}
								onClick={props.onUpParagraph}
								id={paragraph.id.toString()}
								disabled={paragraph.previousId === 0}
							>
								<ExpandLessIcon />
							</IconButton>
						</Grid>
						<Grid item>
							<IconButton
								color={'primary'}
								onClick={props.onDownParagraph}
								id={paragraph.id.toString()}
								disabled={props.isLast}
							>
								<ExpandMoreIcon />
							</IconButton>
						</Grid>
					</Grid>
				</Grid>
				<Grid item>
					<IconButton
						size={'medium'}
						color={paragraph.icon === 'Action' ? 'primary' : ''}
						onClick={props.onParagraphIconChange}
						id={paragraph.id}
						name={'Action'}
					>
						<ArrowCircleRightOutlinedIcon />
					</IconButton>
				</Grid>
				<Grid item>
					<IconButton
						size={'medium'}
						color={paragraph.icon === 'Info' ? 'primary' : ''}
						onClick={props.onParagraphIconChange}
						id={paragraph.id}
						name={'Info'}
					>
						<InfoOutlinedIcon />
					</IconButton>
				</Grid>
				<Grid item>
					<IconButton
						size={'medium'}
						color={paragraph.icon === 'Warning' ? 'primary' : ''}
						onClick={props.onParagraphIconChange}
						id={paragraph.id}
						name={'Warning'}
					>
						<WarningAmberOutlinedIcon />
					</IconButton>
				</Grid>
				<Grid item xs={9}>
					<TextField
						value={paragraph.text}
						id={paragraph.id.toString()}
						onChange={props.onParagraphChange}
						multiline
						fullWidth={true}
						inputProps={{ maxLength: 1000 }}
					/>
				</Grid>
				<Grid item>
					<IconButton
						color={'error'}
						onClick={props.onParagraphDelete}
						id={paragraph.id.toString()}
					>
						<DeleteIcon />
					</IconButton>
				</Grid>
			</Grid>
		</Grid>
	)
}
