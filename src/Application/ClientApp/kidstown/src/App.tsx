import React from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'

import CheckIn from './app/checkin/CheckInPage'
import { AuthProvider } from './app/components/AuthContext'
import Documentation from './app/documentation/Documentation'
import Home from './app/Home'
import Login from './app/login/LoginPage'
import Overview from './app/overview/OverviewPage'
import Settings from './app/settings/SettingsPage'
import Statistic from './app/statistic/StatisticPage'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/documentation" element={<Documentation />} />
          <Route path="/login" element={<Login />} />
          <Route path="/checkin" element={<CheckIn />} />
          <Route path="/overview" element={<Overview />} />
          <Route path="/statistic" element={<Statistic />} />
          <Route path="/settings" element={<Settings />} />
          <Route
            path="/settings/documentation"
            element={<Navigate to="/documentation" replace />}
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
