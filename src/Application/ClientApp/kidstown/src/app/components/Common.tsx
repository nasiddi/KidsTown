import {
  Button,
  ButtonGroup,
  Checkbox,
  FormControlLabel,
  TableCell,
  TextField,
  Tooltip,
} from '@mui/material'
import Grid from '@mui/material/Grid'
import { styled } from '@mui/material/styles'
import type React from 'react'

import { colors } from '../theme'

interface LargeButtonProps {
  id?: string | number
  color?: 'primary' | 'secondary' | 'success' | 'error' | 'info' | 'warning' | 'neutral'
  onClick?: React.MouseEventHandler<HTMLButtonElement>
  disabled?: boolean
  isOutline?: boolean
  name?: React.ReactNode
}

export function LargeButton({ id, color, onClick, disabled, isOutline, name }: LargeButtonProps) {
  return (
    <StyledButton
      id={id?.toString()}
      color={color as 'primary'}
      onClick={onClick}
      disabled={disabled}
      fullWidth={true}
      variant={isOutline ? 'outlined' : 'contained'}
      size={'large'}
    >
      {name}
    </StyledButton>
  )
}

export const StyledTextField = styled(TextField)({
  primary: { main: colors.primary },
})

export const StyledButton = styled(Button)({
  secondary: colors.primary,
  height: 56,
})

export const StyledCheckBox = styled(Checkbox)({
  primary: { main: colors.primary },
})

interface PrimaryCheckBoxProps {
  name: string
  checked: boolean
  onChange: React.ChangeEventHandler<HTMLInputElement>
  label: string
  disabled?: boolean
}

export function PrimaryCheckBox({
  name,
  checked,
  onChange,
  label,
  disabled,
}: PrimaryCheckBoxProps) {
  return (
    <FormControlLabel
      control={
        <StyledCheckBox
          name={name}
          color="primary"
          checked={checked}
          onChange={onChange}
          disabled={disabled}
        />
      }
      label={label}
      labelPlacement="end"
    />
  )
}

interface ToggleButton {
  label: string
  onClick: React.MouseEventHandler<HTMLButtonElement>
  isSelected: boolean
}

interface ToggleButtonsProps {
  buttons: ToggleButton[]
}

export function ToggleButtons({ buttons }: ToggleButtonsProps) {
  const buttonElements = buttons.map((b) => (
    <StyledButton
      key={b.label}
      id={b.label}
      onClick={b.onClick}
      color="primary"
      variant={!b.isSelected ? 'outlined' : 'contained'}
      size={'large'}
    >
      {b.label}
    </StyledButton>
  ))

  return (
    <Grid size={{ md: 3, xs: 12 }}>
      <ButtonGroup size="medium" color="primary">
        {buttonElements}
      </ButtonGroup>
    </Grid>
  )
}

export const HtmlTooltip = styled(Tooltip)({
  backgroundColor: colors.tooltipBg,
  color: colors.tooltipText,
  maxWidth: 500,
  border: `1px solid ${colors.tooltipBorder}`,
  fontsize: '30px',
})

export const TableHeadCell = styled(TableCell)({
  borderBottom: `1.5px solid ${colors.tableBorder}`,
})

export const TableTotalCell = styled(TableCell)({
  borderBottom: `0px solid ${colors.tableBorder}`,
  borderTop: `1.5px solid ${colors.tableBorder}`,
})
