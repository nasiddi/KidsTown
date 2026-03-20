import type React from 'react'

export { paletteOverrideTheme } from '../theme'

export function getSelectedOptionsFromStorage<T>(key: string, fallback: T): T {
  if (localStorage === undefined) {
    return fallback
  }

  const s = localStorage.getItem(key)
  return s === null ? fallback : JSON.parse(s)
}

export function getStringFromSession(key: string, fallback: string): string {
  const s = sessionStorage.getItem(key)

  return s === null ? fallback : s
}

export function getSelectedFromSession<T>(key: string, fallback: T): T {
  const s = sessionStorage.getItem(key)

  return s === null ? fallback : JSON.parse(s)
}

export function getFormattedDate(dateString: string): string {
  const date = new Date(dateString)

  return `${date.getUTCFullYear()}-${`0${date.getUTCMonth() + 1}`.slice(
    -2,
  )}-${`0${date.getUTCDate()}`.slice(-2)}`
}

export function getStateFromLocalStorage(key: string, fallback: boolean): boolean {
  const s = localStorage.getItem(key)

  return s === null ? fallback : JSON.parse(s)
}

export function getLastSunday(): Date {
  const t = new Date()
  t.setDate(t.getDate() - t.getDay())

  return t
}

export function getGuid(): string {
  let deviceGuid = localStorage.getItem('deviceGuid')
  if (deviceGuid === null) {
    deviceGuid = createGuid()
    localStorage.setItem('deviceGuid', deviceGuid)
  }

  return deviceGuid
}

export function getEventId(event: React.MouseEvent): number {
  let id = parseInt(event.currentTarget.id, 10)
  if (isNaN(id)) {
    id = getId(event.currentTarget as HTMLElement)
  }

  return id
}

export function getId(element: HTMLElement): number {
  const id = parseInt(element.parentElement!.id, 10)
  if (!isNaN(id)) {
    return id
  }

  return getId(element.parentElement!)
}

function createGuid(): string {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
    const r = (Math.random() * 16) | 0,
      v = c === 'x' ? r : (r & 0x3) | 0x8

    return v.toString(16)
  })
}
