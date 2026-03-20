import { AppBar, Button, Toolbar, Typography } from '@mui/material'
import React from 'react'
import { useLocation, useNavigate } from 'react-router-dom'

export function NavMenu() {
  const navigate = useNavigate()
  const { pathname } = useLocation()

  const isSelected = (location: string): boolean => {
    if (!location) return pathname === '/'
    return pathname.toLowerCase().includes(location.toLowerCase())
  }

  const onRouteChange = (event: React.MouseEvent<HTMLButtonElement>) => {
    const route = event.currentTarget.name ?? ''
    const path = route ? `/${route}` : '/'
    navigate(path)
  }

  const getNavButton = (route: string, label: string) => {
    const selected = isSelected(route)

    return (
      <Button
        sx={{ minWidth: 90, px: 1 }}
        disableElevation
        name={route}
        color={'white' as 'primary'}
        variant={selected ? 'outlined' : 'contained'}
        onClick={onRouteChange}
      >
        {label ?? route}
      </Button>
    )
  }

  return (
    <AppBar>
      <Toolbar variant="dense">
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          KidsTown
        </Typography>
        {getNavButton('checkin', 'CheckIn/Out')}
        {getNavButton('overview', 'Overview')}
        {getNavButton('statistic', 'Statistic')}
        {getNavButton('', 'Manual')}
      </Toolbar>
    </AppBar>
  )
}
