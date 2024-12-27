'use client'

export function sortDocElements(arr) {
    const sorted = []

    let nextElement = arr.find((e) => e.previousId === 0)
    let previousId = 0

    while (nextElement) {
        sorted.push(nextElement)
        previousId = nextElement.id
        nextElement = findNextElement(previousId, arr)
    }

    return sorted
}

function findNextElement(previousId, docs) {
    return docs.find((e) => e.previousId === previousId)
}

export function onDownSubElement(subElements, targetId) {
    const subElement = subElements.find((e) => e.id?.toString() === targetId)
    const previous = subElements.find((e) => e.id === subElement.previousId)
    const following = subElements.find((e) => e.previousId === subElement.id)

    const followingFollowing = subElements.find(
        (e) => e.previousId === following.id
    )

    following.previousId = previous?.id || 0
    subElement.previousId = following.id

    if (followingFollowing) {
        followingFollowing.previousId = subElement.id
    }
}

export function onUpSubElement(subElements, targetId) {
    const subElement = subElements.find((e) => e.id.toString() === targetId)
    const previous = subElements.find((e) => e.id === subElement.previousId)
    const following = subElements.find((e) => e.previousId === subElement.id)
    const upperPreviousId = previous.previousId

    previous.previousId = subElement.id
    subElement.previousId = upperPreviousId

    if (following) {
        following.previousId = previous.id
    }
}

export function onDeleteSubElement(subElements, targetId) {
    const subElement = subElements.find((e) => e.id === targetId)
    const previous = subElements.find((e) => e.id === subElement.previousId)
    const following = subElements.find((e) => e.previousId === subElement.id)

    const index = subElements.findIndex((e) => e.id === subElement.id)
    subElements.splice(index, 1)

    if (following) {
        following.previousId = previous?.id || 0
    }
}
