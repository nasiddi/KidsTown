import React from 'react'
import { Route, Routes } from 'react-router'
import { CheckInLayout, OverviewLayout } from './components/Layout'

import { library } from '@fortawesome/fontawesome-svg-core'
import { faMobileAlt, faStar } from '@fortawesome/free-solid-svg-icons'

library.add(faMobileAlt)
library.add(faStar)
import './custom-theme.scss'

import './custom.css'
import { CheckInOut } from './components/CheckIn'
import { GuestCheckIn } from './components/GuestCheckIn'
import { Statistics } from './components/Statistic'
import { Settings } from './components/Settings'
import { OverviewOptions } from './components/OverviewOptions'
import { OverviewHeadCount } from './components/OverViewHeadCounts'
import { OverviewFilter } from './components/OverviewFilter'
import { OverviewDetail } from './components/OverviewDetail'
import { Documentation } from './components/Documentation'

export const App = () => {
	return (
		<div>
			<CheckInLayout>
				<Routes>
					<Route exact path="/" element={<Documentation />} />
					<Route path="/statistic" element={<Statistics />} />
					<Route path="/settings" element={<Settings />} />
					<Route
						path="/overview"
						element={
							<>
								<OverviewOptions />
								<OverviewHeadCount />
								<OverviewFilter />
							</>
						}
					/>
					<Route path="/checkin" element={<CheckInOut />} />
					<Route path="/guest" element={<GuestCheckIn />} />
				</Routes>
			</CheckInLayout>
			<OverviewLayout>
				<Routes>
					<Route path="/overview" element={<OverviewDetail />} />
				</Routes>
			</OverviewLayout>
		</div>
	)
}
