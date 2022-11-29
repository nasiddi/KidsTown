import React from 'react'
import { createTheme, Grid, ThemeProvider } from '@mui/material'
import { LargeButton, StyledTextField } from '../Common'

const theme = createTheme({
	palette: {
		neutral: {
			main: '#64748B',
			contrastText: '#fff',
		},
	},
})

export function CheckInInput(props) {
	return (
		<Grid item xs={12} key={props}>
			<Grid
				container
				spacing={1}
				justifyContent="space-between"
				alignItems="center"
			>
				<Grid item md={8} xs={12}>
					<StyledTextField
						inputRef={props.inputRef}
						id="outlined-basic"
						label="SecurityCode"
						variant="outlined"
						value={props.securityCode}
						onChange={props.onChange}
						fullWidth={true}
						InputLabelProps={{
							shrink: true,
						}}
						autoFocus
					/>
				</Grid>
				<Grid item md={2} xs={12}>
					<LargeButton
						id={'search'}
						name={'Search'}
						color="primary"
						onClick={props.onSubmit}
					/>
				</Grid>
				<Grid item md={2} xs={12}>
					<ThemeProvider theme={theme}>
						<LargeButton
							id={'clear'}
							name={'Clear'}
							color="neutral"
							onClick={props.onClear}
						/>
					</ThemeProvider>
				</Grid>
			</Grid>
		</Grid>
	)
}
