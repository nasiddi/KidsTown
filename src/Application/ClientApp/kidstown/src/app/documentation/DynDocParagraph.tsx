import ArrowCircleRightOutlinedIcon from '@mui/icons-material/ArrowCircleRightOutlined'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'

const action = 'action'
const info = 'info'
const warning = 'warning'

interface DocsIconProps {
  name: string
}

export function DocsIcon({ name }: DocsIconProps) {
  if (name === action) {
    return <ArrowCircleRightOutlinedIcon />
  }

  if (name === info) {
    return <InfoOutlinedIcon />
  }

  return <WarningAmberOutlinedIcon />
}

interface DynDocParagraphProps {
  icon: string
  text: string
}

export function DynDocParagraph({ icon, text }: DynDocParagraphProps) {
  let iconElement = <div />
  if (icon === 'Info') {
    iconElement = (
      <div className="wrap-icon">
        <DocsIcon name={info} />
      </div>
    )
  }
  if (icon === 'Action') {
    iconElement = (
      <div className="wrap-icon">
        <DocsIcon name={action} />
      </div>
    )
  }

  if (icon === 'Warning') {
    iconElement = (
      <div className="wrap-icon">
        <DocsIcon name={warning} />
      </div>
    )
  }

  return (
    <div>
      {iconElement}
      <p>
        {text}
        <br />
      </p>
    </div>
  )
}
