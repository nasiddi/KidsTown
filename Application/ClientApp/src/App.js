import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { CheckIn } from './components/CheckIn';

import './custom.css'
import {withAuth} from "./auth/MsalAuthProvider";

class RootApp extends Component {
  static displayName = 'Kidstown';

  render () {
    return (
      <Layout>
        <Route path='/' component={CheckIn} />
      </Layout>
    );
  }
}

export const App = withAuth(RootApp);

