import React from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './app/components/AuthContext'

import Home from './app/page'
import Documentation from './app/documentation/page'
import Login from './app/login/page'
import CheckIn from './app/checkin/page'
import Overview from './app/overview/page'
import Statistic from './app/statistic/page'
import Settings from './app/settings/page'

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
