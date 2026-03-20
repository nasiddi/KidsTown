import React from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'

// Pages (kept in their original locations)
import CheckInPage from '../checkin/CheckInPage'
import Documentation from '../documentation/Documentation'
import Home from '../Home'
import LoginPage from '../login/LoginPage'
import OverviewPage from '../overview/OverviewPage'
import SettingsPage from '../settings/SettingsPage'
import StatisticPage from '../statistic/StatisticPage'

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/documentation" element={<Documentation />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/checkin" element={<CheckInPage />} />
      <Route path="/overview" element={<OverviewPage />} />
      <Route path="/statistic" element={<StatisticPage />} />
      <Route path="/settings" element={<SettingsPage />} />

      {/* Back-compat for an old/unused link in Settings */}
      <Route path="/settings/documentation" element={<Navigate to="/documentation" replace />} />

      {/* Catch-all */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
