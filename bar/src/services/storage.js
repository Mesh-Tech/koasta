export default class StorageService {
  invalidate() {
    window.sessionStorage.clear()
  }

  set(key, value) {
    window.sessionStorage.setItem(key, JSON.stringify(value))
  }

  get(key) {
    const item = window.sessionStorage.getItem(key)
    if (!item) {
      return undefined
    }

    return JSON.parse(item)
  }
}
