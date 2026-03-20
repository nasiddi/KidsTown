import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown'
import { Box, Chip, ClickAwayListener, Menu, MenuItem } from '@mui/material'
import Grid from '@mui/material/Grid'
import type React from 'react'
import { useEffect, useState } from 'react'

import { colors } from '../theme'

import type { SelectOption } from './MultiSelectHelpers'

interface MultiSelectProps {
  options: SelectOption[]
  selectedOptions: SelectOption[]
  onSelectOption: (event: React.MouseEvent<HTMLLIElement>) => void
  onRemoveOption: (event: React.SyntheticEvent) => void
  height?: string
}

export default function MultiSelect({
  options,
  selectedOptions,
  onSelectOption,
  onRemoveOption,
  height,
}: MultiSelectProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setAnchorEl(null)
  }, [options, selectedOptions, onSelectOption, onRemoveOption, height])

  const openLocationMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  function getMenuItems() {
    const menuItems = options.filter(function (l) {
      return !selectedOptions.some((e) => e.value === l.value)
    })

    if (menuItems.length === 0) {
      return <MenuItem disabled={true}>No Options</MenuItem>
    }

    return menuItems.map((l) => (
      <MenuItem id={l.value.toString()} key={l.value} onClick={onSelectOption}>
        {l.label}
      </MenuItem>
    ))
  }

  function renderChips() {
    return selectedOptions.length === 0 ? (
      <Grid sx={{ paddingTop: '5px' }}>
        <span style={{ color: colors.placeholder }}>Select Locations</span>
      </Grid>
    ) : (
      selectedOptions.map((l) => (
        <Grid key={l.value}>
          <Chip label={l.label} id={l.value.toString()} onDelete={onRemoveOption} />
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
              borderColor: colors.placeholder,
              borderRadius: '4px',
              borderStyle: 'solid',
              borderWidth: '1px',
              minHeight: height ?? '34px',
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
                    color: colors.placeholder,
                    float: 'right',
                    marginTop: selectedOptions.length === 0 ? '3px' : 0,
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
