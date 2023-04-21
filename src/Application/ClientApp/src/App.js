import React from 'react'
import { Route, Routes } from 'react-router'
import './custom.css'
import { CheckInOut } from './components/CheckIn'
import { Statistics } from './components/Statistic'
import { Settings } from './components/Settings'
import { NavMenu } from './components/NavMenu'
import { ThemeProvider } from '@mui/material'
import { paletteOverrideTheme } from './components/Common'
import { OverView } from './components/OverView/OverView'
import DynDocumentation from './components/Documentation/DynDocumentation'
import { DynDocumentationEdit } from './components/Documentation/DynDocumentationEdit'

export const App = () => {
	return (
		<div>
			<ThemeProvider theme={paletteOverrideTheme}>
				<NavMenu />
				<br />
				<Routes>
					<Route index element={<DynDocumentation />} />
					<Route path="/statistic" element={<Statistics />} />
					<Route path="/settings" element={<Settings />} />
					<Route
						path="/settings/documentation"
						element={<DynDocumentationEdit />}
					/>
					<Route path="/overview" element={<OverView />} />
					<Route path="/checkin" element={<CheckInOut />} />
				</Routes>
			</ThemeProvider>
		</div>
	)
}
