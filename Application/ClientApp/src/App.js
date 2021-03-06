import React, { Component } from 'react';
import { Route } from 'react-router';
import { CheckInLayout, OverviewLayout } from './components/Layout';
import { CheckIn } from './components/CheckIn';
import { Overview } from "./components/Overview";
import {Statistic} from "./components/Statistic";

import './custom.css'
import {withAuth} from "./auth/MsalAuthProvider";

class RootApp extends Component {
  static displayName = 'Kidstown';

  render () {
      return (
          <div>
              <CheckInLayout>
                  <Route exact path='/' component={CheckIn} />
                  <Route path='/overview' component={Overview} />
                  <Route path='/statistic' component={Statistic} />
              </CheckInLayout>
          </div>
      );
  }
}

export const App = withAuth(RootApp);

