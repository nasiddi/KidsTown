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

export function getFormattedDate(dateString){
    let date = new Date(dateString);
    
    return `${date.getUTCFullYear()}-${("0" + (date.getUTCMonth()+1)).slice(-2)}-${("0" + date.getUTCDate()).slice(-2)}`;
}