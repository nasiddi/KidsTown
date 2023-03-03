import React from 'react'
import { Route, Routes } from 'react-router'
import './custom.css'
import { CheckInOut } from './components/CheckIn'
import { Statistics } from './components/Statistic'
import { Settings } from './components/Settings'
import Documentation from './components/Documentation'
import { NavMenu } from './components/NavMenu'
import { ThemeProvider } from '@mui/material'
import { paletteOverrideTheme } from './components/Common'
import { OverView } from './components/OverView/OverView'

export const App = () => {
	return (
		<div>
			<ThemeProvider theme={paletteOverrideTheme}>
				<NavMenu />
				<br />
				<Routes>
					<Route index element={<Documentation />} />
					<Route path="/statistic" element={<Statistics />} />
					<Route path="/settings" element={<Settings />} />
					<Route path="/overview" element={<OverView />} />
					<Route path="/checkin" element={<CheckInOut />} />
				</Routes>
			</ThemeProvider>
		</div>
	)
}
