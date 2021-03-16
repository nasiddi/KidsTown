import React from 'react'
import { Route } from 'react-router'
import { CheckInLayout, OverviewLayout } from './components/Layout'

import './custom.css'
import { CheckInOut } from './components/CheckIn'
import { Statistics } from './components/Statistic'
import { Settings } from './components/Settings'
import { OverviewOptions } from './components/OverviewOptions'
import { OverviewHeadCount } from './components/OverViewHeadCounts'
import { OverviewDetail } from './components/OverviewDetail'
import { Documentation } from './components/Documentation'

export const App = () => {
	return (
		<div>
			<CheckInLayout>
				<Route exact path="/" component={Documentation} />
				<Route path="/statistic" component={Statistics} />
				<Route path="/settings" component={Settings} />
				<Route path="/overview" component={OverviewOptions} />
				<Route path="/overview" component={OverviewHeadCount} />
				<Route path="/checkin" component={CheckInOut} />
			</CheckInLayout>
			<OverviewLayout>
				<Route path="/overview" component={OverviewDetail} />
			</OverviewLayout>
		</div>
	)
}
