import React from 'react'
import { Grid, MuiThemeProvider } from '@material-ui/core'
import { LargeButton, primaryTheme } from '../Common'
import TextField from '@material-ui/core/TextField'

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
					<MuiThemeProvider theme={primaryTheme}>
						<TextField
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
					</MuiThemeProvider>
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
					<LargeButton
						id={'clear'}
						name={'Clear'}
						color="secondary"
						onClick={props.onClear}
					/>
				</Grid>
			</Grid>
		</Grid>
	)
}
