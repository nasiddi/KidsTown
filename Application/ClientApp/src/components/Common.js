export async function fetchLocations() {
    const response = await fetch('checkinout/location');
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