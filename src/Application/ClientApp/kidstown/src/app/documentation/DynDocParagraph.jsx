import React from 'react'
import ArrowCircleRightOutlinedIcon from '@mui/icons-material/ArrowCircleRightOutlined'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'

const action = 'action'
const info = 'info'
const warning = 'warning'

export function DocsIcon(props) {
  if (props.name === action) {
    return <ArrowCircleRightOutlinedIcon />
  }

  if (props.name === info) {
    return <InfoOutlinedIcon />
  }

  return <WarningAmberOutlinedIcon />
}

export function DynDocParagraph(props) {
  let icon = <div />
  if (props.icon === 'Info') {
    icon = (
      <div className="wrap-icon">
        <DocsIcon name={info} />
      </div>
    )
  }
  if (props.icon === 'Action') {
    icon = (
      <div className="wrap-icon">
        <DocsIcon name={action} />
      </div>
    )
  }

  if (props.icon === 'Warning') {
    icon = (
      <div className="wrap-icon">
        <DocsIcon name={warning} />
      </div>
    )
  }

  return (
    <div>
      {icon}
      <p>
        {props.text}
        <br />
      </p>
    </div>
  )
}
