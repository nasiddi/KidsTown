import {createMuiTheme} from "@material-ui/core";

export async function fetchLocations() {
    const response = await fetch('configuration/locations');
    return await response.json();
}

export const selectStyles = {
    menu: (base) => ({
        ...base,
        zIndex: 100,
    }),
};

export function getSelectedOptionsFromStorage(key, fallback) {
    let s = localStorage.getItem(key);
    return s === null ? fallback : JSON.parse(s);
}

export function getStringFromSession(key, fallback) {
    let s = sessionStorage.getItem(key);
    return s === null ? fallback : s;
}

export function getFormattedDate(dateString){
    let date = new Date(dateString);
    
    return `${date.getUTCFullYear()}-${("0" + (date.getUTCMonth()+1)).slice(-2)}-${("0" + date.getUTCDate()).slice(-2)}`;
}

export const theme = createMuiTheme({
    palette: {
        primary: { main: '#047bff' }
    }});

export function getStateFromLocalStorage(boolean) {
    let s = localStorage.getItem(boolean);
    return s === undefined ? false : JSON.parse(s);
}

export async function getSelectedEventFromStorage() {
    let s = localStorage.getItem('selectedEvent');

    if (s === null) {
        return await fetch('configuration/events/default')
            .then((r) => r.json())
            .then((j) => j['eventId']);
    } else {
        return JSON.parse(s);
    }
}

export function getLastSunday() {
    const t = new Date();
    t.setDate(t.getDate() - t.getDay());
    return t;
}