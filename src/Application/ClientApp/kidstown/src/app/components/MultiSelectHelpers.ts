import type React from 'react'

import type { LocationGroupOption } from '../helpers/BackendClient'

export interface SelectOption {
  value: number
  label: string
}

const updateOptions = (
  options: SelectOption[],
  key: string,
  state: any,
  setState: (s: any) => void,
) => {
  localStorage.setItem(key, JSON.stringify(options))
  setState({ ...state, [key]: options })
}

export function getOnDeselectId(event: React.MouseEvent | React.SyntheticEvent): number {
  const target = event.target as HTMLElement
  let id = target.parentElement!.id
  if (id.length === 0) {
    id = target.parentElement!.parentElement!.id
  }

  return parseInt(id, 10)
}

export function onDeselect(
  event: React.MouseEvent | React.SyntheticEvent,
  optionKey: string,
  state: any,
  setState: (s: any) => void,
) {
  const id = getOnDeselectId(event)

  const options = state[optionKey].filter((l: SelectOption) => l.value !== id)

  updateOptions(options, optionKey, state, setState)
}

export function onSelect(
  event: React.MouseEvent,
  optionKey: string,
  allOptions: LocationGroupOption[],
  state: any,
  setState: (s: any) => void,
) {
  const target = event.target as HTMLElement
  const location = allOptions.find((l) => l.value.toString() === target.id)
  const options = state[optionKey]
  options.push(location)
  updateOptions(options, optionKey, state, setState)
}
