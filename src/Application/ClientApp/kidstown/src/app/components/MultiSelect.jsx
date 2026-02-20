import React, { useEffect, useState } from 'react'
import { Box, Chip, ClickAwayListener, Menu, MenuItem } from '@mui/material'
import Grid from '@mui/material/Grid'
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown'

const updateOptions = (options, key, state, setState) => {
  localStorage.setItem(key, JSON.stringify(options))
  setState({ ...state, [key]: options })
}

export function getOnDeselectId(event) {
  let id = event.target.parentElement.id
  if (id.length === 0) {
    id = event.target.parentElement.parentElement.id
  }

  return parseInt(id, 10)
}

export function onDeselect(event, optionKey, state, setState) {
  const id = getOnDeselectId(event)

  const options = state[optionKey].filter((l) => l.value !== id)

  updateOptions(options, optionKey, state, setState)
}

export function onSelect(event, optionKey, allOptions, state, setState) {
  const location = allOptions.find((l) => l.value.toString() === event.target.id)
  const options = state[optionKey]
  options.push(location)
  updateOptions(options, optionKey, state, setState)
}

export default function MultiSelect(props) {
  const [anchorEl, setAnchorEl] = useState(null)

  useEffect(() => {
    setAnchorEl(null)
  }, [props])

  const openLocationMenu = (event) => {
    setAnchorEl(event.currentTarget)
  }

  function getMenuItems() {
    const menuItems = props.options.filter(function (l) {
      return !props.selectedOptions.some((e) => e.value === l.value)
    })

    if (menuItems.length === 0) {
      return <MenuItem disabled={true}>No Options</MenuItem>
    }

    return menuItems.map((l) => (
      <MenuItem id={l.value} key={l.value} onClick={props.onSelectOption}>
        {l.label}
      </MenuItem>
    ))
  }

  function renderChips() {
    return props.selectedOptions.length === 0 ? (
      <Grid sx={{ paddingTop: '5px' }}>
        <span style={{ color: '#bfbfbf' }}>Select Locations</span>
      </Grid>
    ) : (
      props.selectedOptions.map((l) => (
        <Grid key={l.value}>
          <Chip label={l.label} id={l.value} onDelete={props.onRemoveOption} />
        </Grid>
      ))
    )
  }

  return (
    <>
      <ClickAwayListener onClickAway={() => setAnchorEl(null)}>
        <div
          onClick={(e) => {
            e.stopPropagation()
          }}
        >
          <Box
            sx={{
              borderColor: '#bfbfbf',
              borderRadius: '4px',
              borderStyle: 'solid',
              borderWidth: '1px',
              minHeight: props.height ?? '34px',
              padding: '10px',
            }}
            onClick={openLocationMenu}
          >
            <Grid
              container
              direction="row"
              justifyContent="space-between"
              alignItems="center"
              spacing={1}
            >
              <Grid size={11}>
                <Grid container spacing={1}>
                  {renderChips()}
                </Grid>
              </Grid>
              <Grid size={1}>
                <KeyboardArrowDownIcon
                  style={{
                    color: '#bfbfbf',
                    float: 'right',
                    marginTop: props.selectedOptions.length === 0 ? '3px' : 0,
                  }}
                  onClick={openLocationMenu}
                />
              </Grid>
            </Grid>
          </Box>
        </div>
      </ClickAwayListener>
      <Menu
        disableAutoFocusItem
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'right',
        }}
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={() => setAnchorEl(null)}
      >
        {getMenuItems()}
      </Menu>
    </>
  )
}
