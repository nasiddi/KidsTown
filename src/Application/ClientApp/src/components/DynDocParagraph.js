import React from 'react'
import { DocsIcon } from './DocElements'

const action = 'action'
const info = 'info'
const warning = 'warning'

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
