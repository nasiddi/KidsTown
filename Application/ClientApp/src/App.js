import React, { Component } from 'react';
import { Route } from 'react-router';
import { CheckInLayout, OverviewLayout } from './components/Layout';
import { CheckIn } from './components/CheckIn';
import {Statistic} from "./components/Statistic";

import './custom.css'
import {withAuth} from "./auth/MsalAuthProvider";
import {Settings} from "./components/Settings";
import {OverviewOptions} from "./components/OverviewOptions";
import {OverViewHeadCounts} from "./components/OverViewHeadCounts";
import {OverviewDetail} from "./components/OverviewDetail";

class RootApp extends Component {
  static displayName = 'Kidstown';

  render () {
      return (
          <div>
              <CheckInLayout>
                  <Route exact path='/' component={CheckIn} />
                  <Route path='/statistic' component={Statistic} />
                  <Route path='/settings' component={Settings} />
                  <Route path='/overview' component={OverviewOptions} />
                  <Route path='/overview' component={OverViewHeadCounts} />
              </CheckInLayout>
              <OverviewLayout>
                  <Route path='/overview' component={OverviewDetail} />
              </OverviewLayout>
          </div>
      );
  }
}

export const App = withAuth(RootApp);

