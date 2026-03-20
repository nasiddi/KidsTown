import ReplayIcon from '@mui/icons-material/Replay'
import type { AlertColor } from '@mui/material'
import { Alert, IconButton } from '@mui/material'
import Grid from '@mui/material/Grid'

import type { AlertInfo } from '../helpers/BackendClient'

interface KidsTownAlertProps {
  alert: AlertInfo
  showUndoLink: boolean
  onUndo?: () => void
  callback?: () => void
}

export function KidsTownAlert({ alert, showUndoLink, onUndo, callback }: KidsTownAlertProps) {
  return (
    <Grid size={12}>
      <Alert
        severity={String(alert.level).toLowerCase() as AlertColor}
        action={
          showUndoLink ? (
            <IconButton color="inherit" onClick={onUndo}>
              <ReplayIcon onClick={callback} />
            </IconButton>
          ) : (
            <></>
          )
        }
      >
        {alert.text}
      </Alert>
    </Grid>
  )
}
